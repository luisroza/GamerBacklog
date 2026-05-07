namespace GameBacklog.Application;

public interface IExternalGameLibraryProvider
{
    string Name { get; }
    Task<IReadOnlyList<ExternalLibraryGame>> GetPlayedGamesAsync(string userIdentifier, CancellationToken cancellationToken = default);
}
