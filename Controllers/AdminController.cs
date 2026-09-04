using GamerBacklog.Data;
using GamerBacklog.Domain.Entities;
using GamerBacklog.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GamerBacklog.Controllers;

/// <summary>
/// Hidden admin panel: reachable only by typing /admin_panel (no links in the UI)
/// and only for accounts in the "Admin" role.
/// </summary>
[Authorize(Roles = "Admin")]
[Route("admin_panel")]
public class AdminController : Controller
{
    private const int PageSize = 25;

    private readonly AppDbContext _db;

    public AdminController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(string? search, int page = 1)
    {
        var query = _db.Games.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(g =>
                g.Name.Contains(term) ||
                (g.Developer != null && g.Developer.Contains(term)));
        }

        var total = await query.CountAsync();
        var totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)PageSize));
        page = Math.Clamp(page, 1, totalPages);

        var rows = await query
            .OrderBy(g => g.Name)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .Select(g => new AdminGameRow
            {
                Id = g.Id,
                Name = g.Name,
                CoverUrl = g.CoverUrl,
                ImageUrl = g.ImageUrl,
                ReleaseYear = g.ReleaseYear,
                Developer = g.Developer,
                Metacritic = g.Metacritic,
                RawgId = g.RawgId,
                SteamAppId = g.SteamAppId,
                LibraryCount = g.UserGames.Count
            })
            .ToListAsync();

        var vm = new AdminDashboardViewModel
        {
            Search = search,
            Page = page,
            TotalPages = totalPages,
            TotalCount = total,
            Games = rows,
            TotalGames = await _db.Games.CountAsync(),
            RawgGames = await _db.Games.CountAsync(g => g.RawgId != null),
            SteamGames = await _db.Games.CountAsync(g => g.SteamAppId != null),
            DemoGames = await _db.Games.CountAsync(g => g.RawgId == null && g.SteamAppId == null),
            GamesWithMetacritic80Plus = await _db.Games.CountAsync(g => g.Metacritic >= 80),
            TotalUsers = await _db.Users.CountAsync(),
            LibraryEntries = await _db.UserGames.CountAsync(),
            TotalReviews = await _db.UserGames.CountAsync(ug => ug.Review != null && ug.Review != "")
        };

        return View(vm);
    }

    [HttpGet("create")]
    public IActionResult Create()
    {
        return View(new AdminGameFormViewModel());
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdminGameFormViewModel form)
    {
        if (form.ReleaseYear != null && form.ReleaseYear > DateTime.UtcNow.Year + 2)
        {
            ModelState.AddModelError(nameof(AdminGameFormViewModel.ReleaseYear),
                $"Release year must be at most {DateTime.UtcNow.Year + 2}.");
        }

        if (!ModelState.IsValid) return View(form);

        var game = new Game
        {
            Name = form.Name.Trim(),
            Developer = form.Developer?.Trim() ?? "",
            ReleaseYear = form.ReleaseYear,
            Metacritic = form.Metacritic,
            Genres = form.Genres?.Trim() ?? "",
            Platforms = form.Platforms?.Trim() ?? "",
            Summary = form.Summary?.Trim() ?? "",
            ImageUrl = string.IsNullOrWhiteSpace(form.ImageUrl) ? null : form.ImageUrl.Trim(),
            CoverUrl = "/covers/" + Uri.EscapeDataString(form.Name.Trim())
        };

        _db.Games.Add(game);
        await _db.SaveChangesAsync();

        TempData["Flash"] = $"Game \"{game.Name}\" created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("edit/{id:int}")]
    public async Task<IActionResult> Edit(int id)
    {
        var game = await _db.Games.FindAsync(id);
        if (game == null) return NotFound();

        return View(new AdminGameFormViewModel
        {
            Id = game.Id,
            Name = game.Name,
            Developer = game.Developer,
            ReleaseYear = game.ReleaseYear,
            Metacritic = game.Metacritic,
            Genres = game.Genres,
            Platforms = game.Platforms,
            ImageUrl = game.ImageUrl,
            Summary = game.Summary
        });
    }

    [HttpPost("edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AdminGameFormViewModel form)
    {
        if (form.ReleaseYear != null && form.ReleaseYear > DateTime.UtcNow.Year + 2)
        {
            ModelState.AddModelError(nameof(AdminGameFormViewModel.ReleaseYear),
                $"Release year must be at most {DateTime.UtcNow.Year + 2}.");
        }

        if (!ModelState.IsValid) return View(form);

        var game = await _db.Games.FindAsync(id);
        if (game == null) return NotFound();

        game.Name = form.Name.Trim();
        game.Developer = form.Developer?.Trim() ?? "";
        game.ReleaseYear = form.ReleaseYear;
        game.Metacritic = form.Metacritic;
        game.Genres = form.Genres?.Trim() ?? "";
        game.Platforms = form.Platforms?.Trim() ?? "";
        game.Summary = form.Summary?.Trim() ?? "";
        game.ImageUrl = string.IsNullOrWhiteSpace(form.ImageUrl) ? null : form.ImageUrl.Trim();

        await _db.SaveChangesAsync();

        TempData["Flash"] = "Game updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("delete/{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var game = await _db.Games
            .Include(g => g.UserGames)
            .Include(g => g.Achievements)
            .FirstOrDefaultAsync(g => g.Id == id);
        if (game == null) return NotFound();

        ViewBag.LibraryCount = game.UserGames.Count;
        ViewBag.AchievementCount = game.Achievements.Count;
        return View(game);
    }

    [HttpPost("delete/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var game = await _db.Games.FindAsync(id);
        if (game == null) return NotFound();

        var name = game.Name;
        _db.Games.Remove(game); // cascades to UserGames, Achievements and their unlocks
        await _db.SaveChangesAsync();

        TempData["Flash"] = $"Game \"{name}\" removed from the catalog.";
        return RedirectToAction(nameof(Index));
    }
}
