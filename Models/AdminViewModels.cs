using System.ComponentModel.DataAnnotations;

namespace GamerBacklog.Models;

public class AdminDashboardViewModel
{
    public int TotalGames { get; set; }
    public int RawgGames { get; set; }
    public int SteamGames { get; set; }
    public int DemoGames { get; set; }
    public int GamesWithMetacritic80Plus { get; set; }
    public int TotalUsers { get; set; }
    public int LibraryEntries { get; set; }
    public int TotalReviews { get; set; }

    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
    public int TotalCount { get; set; }
    public List<AdminGameRow> Games { get; set; } = new();
}

public class AdminGameRow
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string CoverUrl { get; set; } = "";
    public string? ImageUrl { get; set; }
    public int? ReleaseYear { get; set; }
    public string Developer { get; set; } = "";
    public int? Metacritic { get; set; }
    public long? RawgId { get; set; }
    public int? SteamAppId { get; set; }
    public int LibraryCount { get; set; }
}

public class AdminGameFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Name is required.")]
    [MinLength(1, ErrorMessage = "Name is required.")]
    [MaxLength(200, ErrorMessage = "Name must be at most 200 characters.")]
    [Display(Name = "Name")]
    public string Name { get; set; } = "";

    [Display(Name = "Developer")]
    [MaxLength(200)]
    public string? Developer { get; set; }

    [Display(Name = "Release year")]
    [Range(1950, 9999, ErrorMessage = "Release year must be 1950 or later.")]
    public int? ReleaseYear { get; set; }

    [Display(Name = "Metacritic score")]
    [Range(0, 100, ErrorMessage = "Metacritic must be between 0 and 100.")]
    public int? Metacritic { get; set; }

    [Display(Name = "Genres (comma separated)")]
    [MaxLength(300)]
    public string? Genres { get; set; }

    [Display(Name = "Platforms (comma separated)")]
    [MaxLength(300)]
    public string? Platforms { get; set; }

    [Display(Name = "Image URL")]
    [Url(ErrorMessage = "Image URL must be a valid absolute URL.")]
    public string? ImageUrl { get; set; }

    [Display(Name = "Summary")]
    [MaxLength(4000)]
    public string? Summary { get; set; }
}
