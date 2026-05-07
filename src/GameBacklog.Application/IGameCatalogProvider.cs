namespace GameBacklog.Application;

public interface IGameCatalogProvider
{
    string SourceName { get; }
    string Description { get; }
    Task<IReadOnlyList<GameMetadataSearchResult>> SearchAsync(string query, CancellationToken cancellationToken = default);
}
