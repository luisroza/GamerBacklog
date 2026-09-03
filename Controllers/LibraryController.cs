using GamerBacklog.Data;
using GamerBacklog.Domain.Entities;
using GamerBacklog.Domain.Enums;
using GamerBacklog.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GamerBacklog.Controllers;

[Authorize]
public class LibraryController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public LibraryController(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(string? status)
    {
        var userId = _userManager.GetUserId(User)!;

        var all = await _db.UserGames.AsNoTracking()
            .Include(ug => ug.Game)
            .Where(ug => ug.UserId == userId)
            .OrderByDescending(ug => ug.UpdatedAt)
            .ToListAsync();

        var counts = all.GroupBy(ug => ug.Status).ToDictionary(g => g.Key, g => g.Count());

        PlayStatus? selected = null;
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<PlayStatus>(status, true, out var parsed))
        {
            selected = parsed;
        }

        var items = selected == null ? all : all.Where(ug => ug.Status == selected).ToList();

        return View(new LibraryViewModel
        {
            Selected = selected,
            TotalCount = all.Count,
            Counts = counts,
            Items = items
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, int status, string? back)
    {
        var userId = _userManager.GetUserId(User)!;
        var userGame = await _db.UserGames.Include(ug => ug.Game).FirstOrDefaultAsync(ug => ug.Id == id);
        if (userGame == null) return NotFound();
        if (userGame.UserId != userId) return Forbid();

        if (Enum.IsDefined(typeof(PlayStatus), status))
        {
            userGame.Status = (PlayStatus)status;
            userGame.UpdatedAt = DateTime.UtcNow;

            var user = await _db.Users.FindAsync(userId);
            var name = string.IsNullOrWhiteSpace(user?.DisplayName) ? user?.UserName : user!.DisplayName;

            _db.Activities.Add(new Activity
            {
                UserId = userId,
                GameId = userGame.GameId,
                Type = ActivityType.Status,
                Text = $"{name} marked {userGame.Game.Name} as {userGame.Status.Label()}.",
                CreatedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
            TempData["Flash"] = "Status updated!";
        }

        return RedirectToAction(nameof(Index), new { status = back });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int id, string? back)
    {
        var userId = _userManager.GetUserId(User)!;
        var userGame = await _db.UserGames.FirstOrDefaultAsync(ug => ug.Id == id);
        if (userGame == null) return NotFound();
        if (userGame.UserId != userId) return Forbid();

        _db.UserGames.Remove(userGame);
        await _db.SaveChangesAsync();
        TempData["Flash"] = "Game removed from your library.";

        return RedirectToAction(nameof(Index), new { status = back });
    }
}
