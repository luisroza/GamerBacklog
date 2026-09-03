namespace GamerBacklog.Domain.Entities;

public class Follow
{
    public int Id { get; set; }
    public string FollowerId { get; set; } = "";
    public ApplicationUser Follower { get; set; } = null!;
    public string FolloweeId { get; set; } = "";
    public ApplicationUser Followee { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
