namespace GameBacklog.Application;

public interface IGameMetadataProvider
{
    string Name { get; }
    Task<IReadOnlyList<GameMetadataSearchResult>> SearchAsync(string query, CancellationToken cancellationToken = default);
}
