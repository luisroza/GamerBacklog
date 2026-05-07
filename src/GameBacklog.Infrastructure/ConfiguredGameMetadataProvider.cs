using GameBacklog.Application;
using Microsoft.Extensions.Configuration;

namespace GameBacklog.Infrastructure;

public sealed class ConfiguredGameMetadataProvider(IConfiguration configuration, IgdbMetadataProvider igdbMetadataProvider, MockGameMetadataProvider mockGameMetadataProvider) : IGameMetadataProvider
{
    public string Name => ActiveProvider.Name;
    public Task<IReadOnlyList<GameMetadataSearchResult>> SearchAsync(string query, CancellationToken cancellationToken = default) => ActiveProvider.SearchAsync(query, cancellationToken);
    private IGameMetadataProvider ActiveProvider => HasIgdbCredentials ? igdbMetadataProvider : mockGameMetadataProvider;
    private bool HasIgdbCredentials => !string.IsNullOrWhiteSpace(configuration["Igdb:ClientId"]) && !string.IsNullOrWhiteSpace(configuration["Igdb:ClientSecret"]);
}
