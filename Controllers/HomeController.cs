using GamerBacklog.Data;
using GamerBacklog.Domain.Entities;
using GamerBacklog.Domain.Enums;
using GamerBacklog.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GamerBacklog.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public HomeController(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var vm = new HomeIndexViewModel();

        var allGames = await _db.Games.AsNoTracking().Include(g => g.UserGames).ToListAsync();
        vm.Trending = allGames
            .OrderByDescending(g => g.UserGames
                .Where(ug => ug.Rating != null)
                .Select(ug => (double)ug.Rating!.Value)
                .DefaultIfEmpty(0)
                .Average())
            .ThenBy(g => g.Id)
            .Take(10)
            .ToList();

        if (User.Identity?.IsAuthenticated == true)
        {
            vm.IsLoggedIn = true;
            var userId = _userManager.GetUserId(User)!;

            vm.ContinuePlaying = await _db.UserGames.AsNoTracking()
                .Include(ug => ug.Game)
                .Where(ug => ug.UserId == userId && ug.Status == PlayStatus.Playing)
                .OrderByDescending(ug => ug.UpdatedAt)
                .Take(10)
                .ToListAsync();

            var followedIds = await _db.Follows.AsNoTracking()
                .Where(f => f.FollowerId == userId)
                .Select(f => f.FolloweeId)
                .ToListAsync();
            followedIds.Add(userId);

            vm.Feed = await _db.Activities.AsNoTracking()
                .Include(a => a.User)
                .Include(a => a.Game)
                .Where(a => followedIds.Contains(a.UserId))
                .OrderByDescending(a => a.CreatedAt)
                .Take(20)
                .ToListAsync();
        }

        return View(vm);
    }

    public IActionResult Error() => View();
}
