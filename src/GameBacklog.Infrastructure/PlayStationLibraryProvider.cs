using GameBacklog.Application;

namespace GameBacklog.Infrastructure;

public sealed class PlayStationLibraryProvider : IExternalGameLibraryProvider
{
    public string Name => "PlayStation";
    public Task<IReadOnlyList<ExternalLibraryGame>> GetPlayedGamesAsync(string userIdentifier, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<ExternalLibraryGame> games = [new("PlayStation", "psn-1942", "God of War Ragnarok", "PlayStation 5", 960, DateTime.Today.AddDays(-8)), new("PlayStation", "psn-1428", "Alan Wake II", "PlayStation 5", 180, DateTime.Today.AddDays(-30))];
        return Task.FromResult(games);
    }
}
