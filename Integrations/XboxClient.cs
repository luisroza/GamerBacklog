using GamerBacklog.Domain.Entities;

namespace GamerBacklog.Integrations;

/// <summary>
/// Integração Xbox — stub. Será implementada quando houver credenciais disponíveis.
/// </summary>
public class XboxClient : IXboxClient
{
    public Task<SteamSyncResult> SyncAchievementsAsync(ApplicationUser user)
        => throw new NotImplementedException("Xbox integration coming soon.");
}
