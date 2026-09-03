using GamerBacklog.Integrations;
using GamerBacklog.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using GamerBacklog.Domain.Entities;

namespace GamerBacklog.Controllers;

[Authorize]
public class SettingsController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ISteamClient _steam;
    private readonly RawgClient _rawg;

    public SettingsController(UserManager<ApplicationUser> userManager, ISteamClient steam, RawgClient rawg)
    {
        _userManager = userManager;
        _steam = steam;
        _rawg = rawg;
    }

    public async Task<IActionResult> Integrations()
    {
        var me = await _userManager.GetUserAsync(User);
        return View(new IntegrationsViewModel
        {
            SteamId = me?.SteamId,
            SteamConfigured = _steam.IsConfigured,
            LastSync = me?.LastSteamSyncAt,
            RawgConfigured = _rawg.IsConfigured
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveSteam(string? steamId)
    {
        var me = await _userManager.GetUserAsync(User);
        if (me == null) return Challenge();

        me.SteamId = string.IsNullOrWhiteSpace(steamId) ? null : steamId.Trim();
        await _userManager.UpdateAsync(me);
        TempData["Flash"] = me.SteamId == null
            ? "SteamID removed."
            : $"SteamID saved: {me.SteamId}";

        return RedirectToAction(nameof(Integrations));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SyncSteam()
    {
        var me = await _userManager.GetUserAsync(User);
        if (me == null) return Challenge();

        try
        {
            var result = await _steam.SyncAchievementsAsync(me);
            TempData["Flash"] = result.Message;
        }
        catch (NotImplementedException)
        {
            TempData["Flash"] = "This integration is not available yet.";
        }
        catch (Exception)
        {
            TempData["Flash"] = "Could not sync right now. Please try again.";
        }

        return RedirectToAction(nameof(Integrations));
    }
}
