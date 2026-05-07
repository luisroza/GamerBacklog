using GameBacklog.Application;

namespace GameBacklog.Infrastructure;

public sealed class SteamLibraryProvider : IExternalGameLibraryProvider
{
    public string Name => "Steam";
    public Task<IReadOnlyList<ExternalLibraryGame>> GetPlayedGamesAsync(string userIdentifier, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<ExternalLibraryGame> games = [new("Steam", "steam-367520", "Hollow Knight", "Steam", 1320, DateTime.Today.AddDays(-2)), new("Steam", "steam-1245620", "Elden Ring", "Steam", 420, DateTime.Today.AddDays(-21))];
        return Task.FromResult(games);
    }
}
