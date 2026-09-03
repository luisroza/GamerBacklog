using GamerBacklog.Data;
using GamerBacklog.Domain.Entities;
using GamerBacklog.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GamerBacklog.Services;

public interface ILibraryService
{
    Task<UserGame> SaveAsync(string userId, int gameId, PlayStatus? status, int? rating, string? review);
    Task RemoveAsync(string userId, int userGameId);
}

public class LibraryService : ILibraryService
{
    private readonly AppDbContext _db;

    public LibraryService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<UserGame> SaveAsync(string userId, int gameId, PlayStatus? status, int? rating, string? review)
    {
        var game = await _db.Games.FindAsync(gameId)
            ?? throw new InvalidOperationException("Game not found.");
        var user = await _db.Users.FindAsync(userId)
            ?? throw new InvalidOperationException("User not found.");

        var userGame = await _db.UserGames.FirstOrDefaultAsync(x => x.UserId == userId && x.GameId == gameId);
        if (userGame == null)
        {
            userGame = new UserGame
            {
                UserId = userId,
                GameId = gameId,
                Status = PlayStatus.Backlog,
                CreatedAt = DateTime.UtcNow
            };
            _db.UserGames.Add(userGame);
        }

        if (status != null) userGame.Status = status.Value;
        if (rating != null) userGame.Rating = Math.Clamp(rating.Value, 1, 5);
        if (review != null) userGame.Review = string.IsNullOrWhiteSpace(review) ? null : review.Trim();
        userGame.UpdatedAt = DateTime.UtcNow;

        var displayName = string.IsNullOrWhiteSpace(user.DisplayName) ? user.UserName : user.DisplayName;

        if (review != null && !string.IsNullOrWhiteSpace(userGame.Review))
        {
            _db.Activities.Add(new Activity
            {
                UserId = userId,
                GameId = gameId,
                Type = ActivityType.Review,
                Text = $"{displayName} rated {game.Name} {userGame.Rating ?? 0} star{((userGame.Rating ?? 0) == 1 ? "" : "s")}.",
                CreatedAt = DateTime.UtcNow
            });

            var others = await _db.UserGames
                .Where(x => x.GameId == gameId && x.UserId != userId)
                .Select(x => x.UserId)
                .Distinct()
                .ToListAsync();

            foreach (var otherId in others)
            {
                _db.Notifications.Add(new Notification
                {
                    UserId = otherId,
                    Text = $"{displayName} reviewed {game.Name}.",
                    Link = $"/Games/{gameId}",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }
        else if (status != null)
        {
            _db.Activities.Add(new Activity
            {
                UserId = userId,
                GameId = gameId,
                Type = ActivityType.Status,
                Text = $"{displayName} marked {game.Name} as {userGame.Status.Label()}.",
                CreatedAt = DateTime.UtcNow
            });
        }

        await _db.SaveChangesAsync();
        return userGame;
    }

    public async Task RemoveAsync(string userId, int userGameId)
    {
        var userGame = await _db.UserGames.FirstOrDefaultAsync(x => x.Id == userGameId && x.UserId == userId);
        if (userGame == null) return;

        _db.UserGames.Remove(userGame);
        await _db.SaveChangesAsync();
    }
}
