using GameBacklog.Domain;

namespace GameBacklog.Application;

public interface IGameBacklogService
{
    IReadOnlyList<GameEntry> GetGames();
    IReadOnlyList<GameEntry> SearchGames(string? search, GameStatus? status, string? platform);
    IReadOnlyList<GameEntry> GetPriorityBacklog();
    IReadOnlyList<GameEntry> PickNextGames(NextGameCriteria criteria);
    BacklogStats GetStats();
    void UpdateStatus(int gameId, GameStatus status);
    GameEntry AddManualGame(ManualGameRequest request);
    GameEntry AddFromMetadata(GameMetadataSearchResult metadata, string platform, GameStatus status);
    int ImportPlayedGames(IEnumerable<ExternalLibraryGame> games);
}
