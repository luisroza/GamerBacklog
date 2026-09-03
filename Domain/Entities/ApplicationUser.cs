using Microsoft.AspNetCore.Identity;

namespace GamerBacklog.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public string DisplayName { get; set; } = "";
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }
    public string? SteamId { get; set; }
    public DateTime? LastSteamSyncAt { get; set; }
}
