using GameBacklog.Application;
using GameBacklog.Domain;

namespace GameBacklog.Infrastructure;

public sealed class InMemoryGameBacklogService : IGameBacklogService
{
    private readonly object _sync = new();
    private readonly List<GameEntry> _games =
    [
        new(1, "Hollow Knight", "Steam Deck", GameStatus.Playing, OwnershipType.Digital, 1, 68, 4.8, 35, 22, "cover-blue", ["Metroidvania", "Action"], ["finish first", "handheld"], "Close to the end. Good candidate for a focused weekend.", DateTime.Today.AddDays(-92)),
        new(2, "Baldur's Gate 3", "PC", GameStatus.Paused, OwnershipType.Digital, 2, 34, 5.0, 90, 31, "cover-brass", ["RPG", "Tactical"], ["long", "party"], "Amazing, but needs brain space. Keep paused without guilt.", DateTime.Today.AddDays(-210)),
        new(3, "Celeste", "Switch", GameStatus.WantToPlay, OwnershipType.Physical, 1, 0, 0, 9, 0, "cover-rose", ["Platformer"], ["short", "challenge"], "Short, finite, and perfect for the pick-next flow.", DateTime.Today.AddDays(-43)),
        new(4, "Alan Wake II", "PlayStation 5", GameStatus.WantToPlay, OwnershipType.Digital, 3, 0, 0, 18, 0, "cover-green", ["Horror", "Adventure"], ["cinematic", "night"], "Save for a darker mood.", DateTime.Today.AddDays(-18)),
        new(5, "Hi-Fi Rush", "Xbox", GameStatus.Completed, OwnershipType.Subscription, 0, 100, 4.6, 12, 13, "cover-coral", ["Action", "Rhythm"], ["completed", "joy"], "Finished main story. Great tone reference for encouraging stats.", DateTime.Today.AddDays(-155), DateTime.Today.AddDays(-71)),
        new(6, "A Short Hike", "PC", GameStatus.WantToPlay, OwnershipType.Digital, 1, 0, 0, 2, 0, "cover-sky", ["Adventure", "Cozy"], ["tiny", "reset"], "Ideal when there are only a couple free hours.", DateTime.Today.AddDays(-25))
    ];

    public IReadOnlyList<GameEntry> GetGames() => _games;
    public IReadOnlyList<GameEntry> SearchGames(string? search, GameStatus? status, string? platform)
    {
        IEnumerable<GameEntry> query = _games;
        if (!string.IsNullOrWhiteSpace(search)) query = query.Where(g => g.Title.Contains(search, StringComparison.OrdinalIgnoreCase) || g.Platform.Contains(search, StringComparison.OrdinalIgnoreCase) || g.Genres.Any(x => x.Contains(search, StringComparison.OrdinalIgnoreCase)) || g.Tags.Any(x => x.Contains(search, StringComparison.OrdinalIgnoreCase)));
        if (status is not null) query = query.Where(g => g.Status == status);
        if (!string.IsNullOrWhiteSpace(platform) && platform != "All") query = query.Where(g => g.Platform == platform);
        return query.OrderByDescending(g => g.Status == GameStatus.Playing).ThenBy(g => g.Priority == 0 ? int.MaxValue : g.Priority).ThenBy(g => g.Title).ToList();
    }
    public IReadOnlyList<GameEntry> GetPriorityBacklog() => _games.Where(g => g.Status is GameStatus.WantToPlay or GameStatus.Paused).OrderBy(g => g.Priority == 0 ? int.MaxValue : g.Priority).ThenBy(g => g.EstimatedHours).ToList();
    public IReadOnlyList<GameEntry> PickNextGames(NextGameCriteria criteria) => _games.Where(g => g.Status is GameStatus.WantToPlay or GameStatus.Paused or GameStatus.Playing).Where(g => criteria.Platform == "Any" || string.IsNullOrWhiteSpace(criteria.Platform) || g.Platform == criteria.Platform).Where(g => criteria.AvailableHours <= 0 || g.EstimatedHours <= criteria.AvailableHours || g.Status == GameStatus.Playing || g.Tags.Contains("endless")).Take(4).ToList();
    public BacklogStats GetStats()
    {
        var statusCounts = _games.GroupBy(g => g.Status).ToDictionary(g => g.Key, g => g.Count());
        var platformCounts = _games.GroupBy(g => g.Platform).ToDictionary(g => g.Key, g => g.Count());
        var rated = _games.Where(g => g.Rating > 0).ToList();
        return new BacklogStats(_games.Count, statusCounts.GetValueOrDefault(GameStatus.Playing), statusCounts.GetValueOrDefault(GameStatus.WantToPlay) + statusCounts.GetValueOrDefault(GameStatus.Paused), statusCounts.GetValueOrDefault(GameStatus.Completed), statusCounts.GetValueOrDefault(GameStatus.Dropped), statusCounts.GetValueOrDefault(GameStatus.Wishlist), _games.Where(g => g.Status is GameStatus.WantToPlay or GameStatus.Paused).Sum(g => Math.Max(g.EstimatedHours - g.PlayedHours, 0)), _games.Sum(g => g.PlayedHours), rated.Count == 0 ? 0 : Math.Round(rated.Average(g => g.Rating), 1), platformCounts, statusCounts);
    }
    public void UpdateStatus(int gameId, GameStatus status) { lock (_sync) { var i = _games.FindIndex(g => g.Id == gameId); if (i < 0) return; var g = _games[i]; _games[i] = g with { Status = status, ProgressPercent = status == GameStatus.Completed ? 100 : g.ProgressPercent, FinishedOn = status == GameStatus.Completed ? DateTime.Today : g.FinishedOn }; } }
    public GameEntry AddManualGame(ManualGameRequest request) { lock (_sync) { var entry = new GameEntry(_games.Max(g => g.Id) + 1, request.Title.Trim(), request.Platform.Trim(), request.Status, request.Ownership, request.Status == GameStatus.WantToPlay ? 3 : 0, request.Status == GameStatus.Completed ? 100 : 0, 0, Math.Max(request.EstimatedHours, 1), request.Status == GameStatus.Completed ? Math.Max(request.EstimatedHours, 1) : 0, "cover-sky", ["Manual"], ["manual"], string.IsNullOrWhiteSpace(request.Notes) ? "Added manually by the user." : request.Notes.Trim(), DateTime.Today, request.Status == GameStatus.Completed ? DateTime.Today : null); _games.Add(entry); return entry; } }
    public GameEntry AddFromMetadata(GameMetadataSearchResult metadata, string platform, GameStatus status) { lock (_sync) { var selectedPlatform = string.IsNullOrWhiteSpace(platform) ? metadata.Platforms.FirstOrDefault() ?? "Unknown" : platform; var existing = _games.FirstOrDefault(g => g.Title.Equals(metadata.Title, StringComparison.OrdinalIgnoreCase) && g.Platform.Equals(selectedPlatform, StringComparison.OrdinalIgnoreCase)); if (existing is not null) return existing; var entry = new GameEntry(_games.Max(g => g.Id) + 1, metadata.Title, selectedPlatform, status, status == GameStatus.Wishlist ? OwnershipType.Wishlist : OwnershipType.Digital, status == GameStatus.WantToPlay ? 3 : 0, 0, 0, 14, 0, "cover-sky", metadata.Genres, [metadata.Provider.ToLowerInvariant()], metadata.Summary, DateTime.Today); _games.Add(entry); return entry; } }
    public int ImportPlayedGames(IEnumerable<ExternalLibraryGame> games) { var count = 0; foreach (var game in games) { AddFromMetadata(new GameMetadataSearchResult(game.Provider, game.ProviderGameId, game.Title, [game.Platform], ["Imported"], null, null, $"Imported from {game.Provider}."), game.Platform, GameStatus.Playing); count++; } return count; }
}
