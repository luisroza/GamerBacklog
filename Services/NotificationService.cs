using GamerBacklog.Data;
using GamerBacklog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GamerBacklog.Services;

public interface INotificationService
{
    Task NotifyAsync(string userId, string text, string link);
    Task<List<Notification>> GetLatestAsync(string userId, int take = 8);
    Task<int> GetUnreadCountAsync(string userId);
    Task MarkAllReadAsync(string userId);
}

public class NotificationService : INotificationService
{
    private readonly AppDbContext _db;

    public NotificationService(AppDbContext db)
    {
        _db = db;
    }

    public async Task NotifyAsync(string userId, string text, string link)
    {
        if (string.IsNullOrEmpty(userId)) return;

        _db.Notifications.Add(new Notification
        {
            UserId = userId,
            Text = text,
            Link = link,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
    }

    public Task<List<Notification>> GetLatestAsync(string userId, int take = 8) =>
        _db.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(take)
            .ToListAsync();

    public Task<int> GetUnreadCountAsync(string userId) =>
        _db.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);

    public async Task MarkAllReadAsync(string userId)
    {
        var unread = await _db.Notifications.Where(n => n.UserId == userId && !n.IsRead).ToListAsync();
        foreach (var n in unread)
        {
            n.IsRead = true;
        }
        await _db.SaveChangesAsync();
    }
}
