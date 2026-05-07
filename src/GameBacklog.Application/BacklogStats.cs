using GameBacklog.Domain;

namespace GameBacklog.Application;

public sealed record BacklogStats(
    int TotalGames,
    int Playing,
    int Backlog,
    int Completed,
    int Dropped,
    int Wishlist,
    int EstimatedBacklogHours,
    int PlayedHours,
    double AverageRating,
    IReadOnlyDictionary<string, int> PlatformCounts,
    IReadOnlyDictionary<GameStatus, int> StatusCounts);
