namespace GamerBacklog.Domain.Entities;

public class Game
{
    public int Id { get; set; }
    public long? IgdbId { get; set; }
    public long? RawgId { get; set; }
    public string Name { get; set; } = "";
    public string CoverUrl { get; set; } = "";
    public string? ImageUrl { get; set; }
    public string Summary { get; set; } = "";
    public int? ReleaseYear { get; set; }
    public string Platforms { get; set; } = "";
    public string Genres { get; set; } = "";
    public string Developer { get; set; } = "";
    public int? Metacritic { get; set; }
    public int? SteamAppId { get; set; }

    public ICollection<UserGame> UserGames { get; set; } = new List<UserGame>();
    public ICollection<Achievement> Achievements { get; set; } = new List<Achievement>();
}
