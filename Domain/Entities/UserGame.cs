using GamerBacklog.Domain.Enums;

namespace GamerBacklog.Domain.Entities;

public class UserGame
{
    public int Id { get; set; }
    public string UserId { get; set; } = "";
    public ApplicationUser User { get; set; } = null!;
    public int GameId { get; set; }
    public Game Game { get; set; } = null!;
    public PlayStatus Status { get; set; } = PlayStatus.Backlog;
    public int? Rating { get; set; }
    public string? Review { get; set; }
    public int? HoursPlayed { get; set; }
    public bool Favorite { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
