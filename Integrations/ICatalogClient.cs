using GamerBacklog.Domain.Entities;

namespace GamerBacklog.Integrations;

/// <summary>
/// Catalog client. The implementation is chosen at startup:
/// RawgClient when "Rawg:ApiKey" is configured (free API with Metacritic scores),
/// otherwise the built-in DemoIgdbClient catalog.
/// </summary>
public interface ICatalogClient
{
    Task<IReadOnlyList<Game>> GetPopularGamesAsync(int take = 40);
    Task<Game?> GetByIdAsync(long externalId);
}
