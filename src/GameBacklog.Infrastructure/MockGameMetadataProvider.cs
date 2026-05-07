using GameBacklog.Application;

namespace GameBacklog.Infrastructure;

public sealed class MockGameMetadataProvider : IGameMetadataProvider
{
    private static readonly IReadOnlyList<GameMetadataSearchResult> Catalog =
    [
        new("Mock Metadata", "igdb-1942", "Elden Ring", ["PC", "PlayStation 5", "Xbox"], ["Action RPG", "Open World"], 2022, null, "A vast action RPG with flexible builds, long playtime, and high replay value."),
        new("Mock Metadata", "igdb-1970", "Hades", ["PC", "Switch", "PlayStation 5", "Xbox"], ["Roguelite", "Action"], 2020, null, "Fast runs, strong progression, and ideal fit for short sessions."),
        new("Mock Metadata", "igdb-2073", "Baldur's Gate 3", ["PC", "PlayStation 5", "Xbox"], ["RPG", "Tactical"], 2023, null, "Party-based RPG with deep choices and a long campaign."),
        new("Mock Metadata", "igdb-1428", "God of War Ragnarok", ["PlayStation 5", "PlayStation 4", "PC"], ["Action", "Adventure"], 2022, null, "Cinematic action adventure with a finite story path."),
        new("Mock Metadata", "igdb-1185", "Dave the Diver", ["PC", "Switch", "PlayStation 5"], ["Adventure", "Management"], 2023, null, "A relaxed loop of diving, collecting, and restaurant management."),
        new("Mock Metadata", "igdb-901", "Balatro", ["PC", "Switch", "PlayStation 5", "Xbox"], ["Card Game", "Roguelike"], 2024, null, "Compact roguelike card runs with high replayability.")
    ];

    public string Name => "Mock Metadata";

    public Task<IReadOnlyList<GameMetadataSearchResult>> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query)) return Task.FromResult<IReadOnlyList<GameMetadataSearchResult>>([]);
        var results = Catalog.Where(game => game.Title.Contains(query, StringComparison.OrdinalIgnoreCase) || game.Genres.Any(genre => genre.Contains(query, StringComparison.OrdinalIgnoreCase)) || game.Platforms.Any(platform => platform.Contains(query, StringComparison.OrdinalIgnoreCase))).Take(8).ToList();
        return Task.FromResult<IReadOnlyList<GameMetadataSearchResult>>(results);
    }
}
