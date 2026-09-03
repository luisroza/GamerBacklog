namespace GamerBacklog.Domain.Entities;

public class Achievement
{
    public int Id { get; set; }
    public int GameId { get; set; }
    public Game Game { get; set; } = null!;
    public string ExternalId { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string? IconUrl { get; set; }
    public double RarityPercent { get; set; }
}
