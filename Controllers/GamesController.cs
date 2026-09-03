using GamerBacklog.Data;
using GamerBacklog.Domain.Entities;
using GamerBacklog.Domain.Enums;
using GamerBacklog.Integrations;
using GamerBacklog.Models;
using GamerBacklog.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GamerBacklog.Controllers;

[Route("Games")]
public class GamesController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILibraryService _library;
    private readonly RawgClient _rawg;

    public GamesController(AppDbContext db, UserManager<ApplicationUser> userManager, ILibraryService library, RawgClient rawg)
    {
        _db = db;
        _userManager = userManager;
        _library = library;
        _rawg = rawg;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(string? q, string? genre, string? platform, int page = 1)
    {
        var all = await _db.Games.AsNoTracking().Include(g => g.UserGames).ToListAsync();

        IEnumerable<Game> filtered = all;
        if (!string.IsNullOrWhiteSpace(q))
        {
            filtered = filtered.Where(g => g.Name.Contains(q.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        // On-demand import: the user searched for a game we don't have locally,
        // so pull it from RAWG once and store it. Future searches hit the database only.
        if (!string.IsNullOrWhiteSpace(q) && !filtered.Any())
        {
            var imported = await _rawg.ImportSearchResultsAsync(q);
            if (imported > 0)
            {
                all = await _db.Games.AsNoTracking().Include(g => g.UserGames).ToListAsync();
                filtered = all.Where(g => g.Name.Contains(q.Trim(), StringComparison.OrdinalIgnoreCase));
            }
        }
        if (!string.IsNullOrWhiteSpace(genre))
        {
            filtered = filtered.Where(g => g.Genres
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Any(x => x.Equals(genre, StringComparison.OrdinalIgnoreCase)));
        }
        if (!string.IsNullOrWhiteSpace(platform))
        {
            filtered = filtered.Where(g => g.Platforms
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Any(x => x.Equals(platform, StringComparison.OrdinalIgnoreCase)));
        }

        var genres = all
            .SelectMany(g => g.Genres.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .ToList();

        var platforms = all
            .SelectMany(g => g.Platforms.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .ToList();

        var ordered = filtered.OrderBy(g => g.Name).ToList();
        const int pageSize = 20;
        var total = ordered.Count;
        var totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)pageSize));
        page = Math.Clamp(page, 1, totalPages);
        var items = ordered.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        var statuses = new Dictionary<int, PlayStatus?>();
        if (User.Identity?.IsAuthenticated == true)
        {
            var userId = _userManager.GetUserId(User)!;
            statuses = items.ToDictionary(
                g => g.Id,
                g => (PlayStatus?)g.UserGames.FirstOrDefault(ug => ug.UserId == userId)?.Status);
        }

        return View(new GamesIndexViewModel
        {
            Q = q,
            Genre = genre,
            Platform = platform,
            Page = page,
            TotalPages = totalPages,
            TotalCount = total,
            Games = items,
            Genres = genres,
            Platforms = platforms,
            UserStatuses = statuses
        });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Details(int id)
    {
        var game = await _db.Games
            .Include(g => g.UserGames).ThenInclude(ug => ug.User)
            .Include(g => g.Achievements)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (game == null) return NotFound();

        // Lazy detail hydration: description/developer are fetched from RAWG
        // at most once per game (skipped once the Summary is stored).
        if (string.IsNullOrWhiteSpace(game.Summary))
        {
            await _rawg.HydrateGameAsync(game);
        }

        var rated = game.UserGames.Where(ug => ug.Rating != null).ToList();

        var vm = new GameDetailsViewModel
        {
            Game = game,
            AverageRating = rated.Count > 0 ? rated.Average(ug => ug.Rating!.Value) : null,
            RatingCount = rated.Count,
            Reviews = game.UserGames
                .Where(ug => !string.IsNullOrWhiteSpace(ug.Review))
                .OrderByDescending(ug => ug.UpdatedAt)
                .Select(ug => new ReviewItem
                {
                    UserName = ug.User.UserName ?? "user",
                    DisplayName = string.IsNullOrWhiteSpace(ug.User.DisplayName)
                        ? (ug.User.UserName ?? "user")
                        : ug.User.DisplayName,
                    Rating = ug.Rating,
                    Review = ug.Review!,
                    UpdatedAt = ug.UpdatedAt
                })
                .ToList(),
            Achievements = game.Achievements.OrderByDescending(a => a.RarityPercent).ToList()
        };

        if (User.Identity?.IsAuthenticated == true)
        {
            var userId = _userManager.GetUserId(User)!;
            vm.IsLoggedIn = true;
            vm.Existing = game.UserGames.FirstOrDefault(ug => ug.UserId == userId);

            var achievementIds = game.Achievements.Select(a => a.Id).ToList();
            var unlocked = await _db.UserAchievements
                .Where(ua => ua.UserId == userId && achievementIds.Contains(ua.AchievementId))
                .Select(ua => ua.AchievementId)
                .ToListAsync();
            vm.UnlockedIds = unlocked.ToHashSet();
        }

        return View(vm);
    }

    [HttpPost("{id:int}/SetStatus")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetStatus(int id, int status, string? returnUrl)
    {
        if (!Enum.IsDefined(typeof(PlayStatus), status)) return BadRequest();
        if (!await _db.Games.AnyAsync(g => g.Id == id)) return NotFound();

        await _library.SaveAsync(_userManager.GetUserId(User)!, id, (PlayStatus)status, null, null);
        TempData["Flash"] = "Status updated!";

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost("{id:int}/SaveReview")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveReview(int id, int? rating, string? review)
    {
        if (!await _db.Games.AnyAsync(g => g.Id == id)) return NotFound();

        if (rating != null && (rating < 1 || rating > 5))
        {
            TempData["Flash"] = "Rating must be between 1 and 5 stars.";
            return RedirectToAction(nameof(Details), new { id });
        }

        await _library.SaveAsync(_userManager.GetUserId(User)!, id, null, rating, review);
        TempData["Flash"] = "Rating saved!";
        return RedirectToAction(nameof(Details), new { id });
    }
}
