using GamerBacklog.Domain.Entities;

namespace GamerBacklog.Integrations;

public record SteamSyncResult(int NewAchievements, int GamesTouched, bool Simulated, string Message);

/// <summary>
/// Cliente Steam. Se "Steam:ApiKey" estiver configurada no appsettings e o usuário
/// tiver um SteamId salvo, faz chamadas reais à Steam Web API; caso contrário,
/// simula a sincronização de conquistas (30–90% de desbloqueio).
/// </summary>
public interface ISteamClient
{
    bool IsConfigured { get; }
    Task<SteamSyncResult> SyncAchievementsAsync(ApplicationUser user);
}

/// <summary>Integração PSN — stub, será implementada no futuro.</summary>
public interface IPsnClient
{
    Task<SteamSyncResult> SyncAchievementsAsync(ApplicationUser user);
}

/// <summary>Integração Xbox — stub, será implementada no futuro.</summary>
public interface IXboxClient
{
    Task<SteamSyncResult> SyncAchievementsAsync(ApplicationUser user);
}
