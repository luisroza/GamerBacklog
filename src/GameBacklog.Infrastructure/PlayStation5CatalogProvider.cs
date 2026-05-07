using GameBacklog.Application;

namespace GameBacklog.Infrastructure;

public sealed class PlayStation5CatalogProvider(IGameMetadataProvider metadataProvider) : IGameCatalogProvider
{
    public string SourceName => "PlayStation 5 / PSN";
    public string Description => "Searches PS5 games through the configured metadata provider because PSN has no stable public full-catalog API.";

    public async Task<IReadOnlyList<GameMetadataSearchResult>> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query)) return [];
        var results = await metadataProvider.SearchAsync(query, cancellationToken);
        return results.Where(result => result.Platforms.Any(platform => platform.Equals("PlayStation 5", StringComparison.OrdinalIgnoreCase) || platform.Equals("PS5", StringComparison.OrdinalIgnoreCase))).Select(result => result with { Provider = "PlayStation 5 / PSN", Platforms = result.Platforms.Contains("PlayStation 5", StringComparer.OrdinalIgnoreCase) ? result.Platforms : result.Platforms.Concat(["PlayStation 5"]).ToArray(), Summary = $"{result.Summary} Listed as a PlayStation 5 catalog match." }).ToList();
    }
}
