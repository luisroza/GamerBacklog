using GamerBacklog.Data;
using GamerBacklog.Domain.Entities;
using GamerBacklog.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GamerBacklog.Services;

public interface IFollowService
{
    Task<bool> ToggleAsync(ApplicationUser follower, string targetUsername);
}

public class FollowService : IFollowService
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public FollowService(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<bool> ToggleAsync(ApplicationUser follower, string targetUsername)
    {
        var normalized = _userManager.NormalizeName(targetUsername);
        var target = await _db.Users.FirstOrDefaultAsync(u => u.NormalizedUserName == normalized);
        if (target == null || target.Id == follower.Id) return false;

        var existing = await _db.Follows.FirstOrDefaultAsync(f => f.FollowerId == follower.Id && f.FolloweeId == target.Id);
        if (existing != null)
        {
            _db.Follows.Remove(existing);
            await _db.SaveChangesAsync();
            return false;
        }

        var followerName = string.IsNullOrWhiteSpace(follower.DisplayName) ? follower.UserName : follower.DisplayName;

        _db.Follows.Add(new Follow
        {
            FollowerId = follower.Id,
            FolloweeId = target.Id,
            CreatedAt = DateTime.UtcNow
        });

        _db.Notifications.Add(new Notification
        {
            UserId = target.Id,
            Text = $"{followerName} started following you.",
            Link = $"/u/{follower.UserName}",
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        });

        _db.Activities.Add(new Activity
        {
            UserId = follower.Id,
            Type = ActivityType.Follow,
            Text = $"{followerName} started following {target.DisplayName}.",
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        return true;
    }
}
