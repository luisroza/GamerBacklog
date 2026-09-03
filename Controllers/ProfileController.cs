using GamerBacklog.Data;
using GamerBacklog.Domain.Entities;
using GamerBacklog.Models;
using GamerBacklog.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GamerBacklog.Controllers;

[Route("u")]
public class ProfileController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IFollowService _follows;

    public ProfileController(AppDbContext db, UserManager<ApplicationUser> userManager, IFollowService follows)
    {
        _db = db;
        _userManager = userManager;
        _follows = follows;
    }

    [HttpGet("{username}")]
    public async Task<IActionResult> Index(string username)
    {
        var normalized = _userManager.NormalizeName(username);
        var profile = await _db.Users.FirstOrDefaultAsync(u => u.NormalizedUserName == normalized);
        if (profile == null) return NotFound();

        var shelves = await _db.UserGames.AsNoTracking()
            .Include(ug => ug.Game)
            .Where(ug => ug.UserId == profile.Id)
            .ToListAsync();

        var counts = shelves.GroupBy(ug => ug.Status).ToDictionary(g => g.Key, g => g.Count());
        var grouped = shelves
            .GroupBy(ug => ug.Status)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(ug => ug.UpdatedAt).ToList());

        var followers = await _db.Follows.CountAsync(f => f.FolloweeId == profile.Id);
        var following = await _db.Follows.CountAsync(f => f.FollowerId == profile.Id);

        var me = await _userManager.GetUserAsync(User);
        var isSelf = me != null && me.Id == profile.Id;
        var isFollowing = false;
        if (me != null && !isSelf)
        {
            isFollowing = await _db.Follows.AnyAsync(f => f.FollowerId == me.Id && f.FolloweeId == profile.Id);
        }

        return View(new ProfileViewModel
        {
            Profile = profile,
            Counts = counts,
            Shelves = grouped,
            ReviewCount = shelves.Count(ug => !string.IsNullOrWhiteSpace(ug.Review)),
            TotalGames = shelves.Count,
            Followers = followers,
            Following = following,
            IsFollowing = isFollowing,
            IsSelf = isSelf
        });
    }

    [HttpPost("{username}/Follow")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Follow(string username, string? returnUrl)
    {
        var me = await _userManager.GetUserAsync(User);
        if (me == null) return Challenge();

        await _follows.ToggleAsync(me, username);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
        return RedirectToAction(nameof(Index), new { username });
    }
}
