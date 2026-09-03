using GamerBacklog.Domain.Enums;

namespace GamerBacklog.Domain.Entities;

public class Activity
{
    public int Id { get; set; }
    public string UserId { get; set; } = "";
    public ApplicationUser User { get; set; } = null!;
    public ActivityType Type { get; set; }
    public int? GameId { get; set; }
    public Game? Game { get; set; }
    public string Text { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
