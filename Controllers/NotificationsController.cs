using GamerBacklog.Domain.Entities;
using GamerBacklog.Models;
using GamerBacklog.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GamerBacklog.Controllers;

[Authorize]
public class NotificationsController : Controller
{
    private readonly INotificationService _notifications;
    private readonly UserManager<ApplicationUser> _userManager;

    public NotificationsController(INotificationService notifications, UserManager<ApplicationUser> userManager)
    {
        _notifications = notifications;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var me = await _userManager.GetUserAsync(User);
        if (me == null) return Challenge();

        var items = await _notifications.GetLatestAsync(me.Id, 30);
        return View(new NotificationsViewModel { Items = items });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllRead()
    {
        var me = await _userManager.GetUserAsync(User);
        if (me == null) return Challenge();

        await _notifications.MarkAllReadAsync(me.Id);
        return RedirectToAction(nameof(Index));
    }
}
