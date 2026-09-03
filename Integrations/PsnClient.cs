using GamerBacklog.Domain.Entities;

namespace GamerBacklog.Integrations;

/// <summary>
/// Integração PSN — stub. Será implementada quando houver credenciais disponíveis.
/// </summary>
public class PsnClient : IPsnClient
{
    public Task<SteamSyncResult> SyncAchievementsAsync(ApplicationUser user)
        => throw new NotImplementedException("PSN integration coming soon.");
}
