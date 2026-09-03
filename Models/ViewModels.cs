using System.ComponentModel.DataAnnotations;
using GamerBacklog.Domain.Entities;
using GamerBacklog.Domain.Enums;

namespace GamerBacklog.Models;

public class HomeIndexViewModel
{
    public bool IsLoggedIn { get; set; }
    public List<UserGame> ContinuePlaying { get; set; } = new();
    public List<Game> Trending { get; set; } = new();
    public List<Activity> Feed { get; set; } = new();
}

public class GameCardViewModel
{
    public Game Game { get; set; } = null!;
    public PlayStatus? Status { get; set; }
}

public class GamesIndexViewModel
{
    public string? Q { get; set; }
    public string? Genre { get; set; }
    public string? Platform { get; set; }
    public int Page { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
    public int TotalCount { get; set; }
    public List<Game> Games { get; set; } = new();
    public List<string> Genres { get; set; } = new();
    public List<string> Platforms { get; set; } = new();
    public Dictionary<int, PlayStatus?> UserStatuses { get; set; } = new();
}

public class ReviewItem
{
    public string UserName { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public int? Rating { get; set; }
    public string Review { get; set; } = "";
    public DateTime UpdatedAt { get; set; }
}

public class GameDetailsViewModel
{
    public Game Game { get; set; } = null!;
    public UserGame? Existing { get; set; }
    public double? AverageRating { get; set; }
    public int RatingCount { get; set; }
    public List<ReviewItem> Reviews { get; set; } = new();
    public List<Achievement> Achievements { get; set; } = new();
    public HashSet<int> UnlockedIds { get; set; } = new();
    public bool IsLoggedIn { get; set; }
}

public class LibraryViewModel
{
    public PlayStatus? Selected { get; set; }
    public int TotalCount { get; set; }
    public Dictionary<PlayStatus, int> Counts { get; set; } = new();
    public List<UserGame> Items { get; set; } = new();
}

public class ProfileViewModel
{
    public ApplicationUser Profile { get; set; } = null!;
    public Dictionary<PlayStatus, int> Counts { get; set; } = new();
    public Dictionary<PlayStatus, List<UserGame>> Shelves { get; set; } = new();
    public int ReviewCount { get; set; }
    public int TotalGames { get; set; }
    public int Followers { get; set; }
    public int Following { get; set; }
    public bool IsFollowing { get; set; }
    public bool IsSelf { get; set; }
}

public class IntegrationsViewModel
{
    public string? SteamId { get; set; }
    public bool SteamConfigured { get; set; }
    public DateTime? LastSync { get; set; }
    public bool RawgConfigured { get; set; }
}

public class NotificationsViewModel
{
    public List<Notification> Items { get; set; } = new();
}

public class LoginViewModel
{
    [Required(ErrorMessage = "Enter your email or username.")]
    [Display(Name = "Email or username")]
    public string Login { get; set; } = "";

    [Required(ErrorMessage = "Enter your password.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";

    [Display(Name = "Keep me signed in")]
    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}

public class RegisterViewModel
{
    [Required(ErrorMessage = "Choose a username.")]
    [MinLength(3, ErrorMessage = "Username must be at least 3 characters long.")]
    [RegularExpression("^[a-zA-Z0-9_.-]+$", ErrorMessage = "Use only letters, numbers, dot, hyphen or underscore.")]
    public string Username { get; set; } = "";

    [Required(ErrorMessage = "Enter an email.")]
    [EmailAddress(ErrorMessage = "Invalid email.")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Choose a password.")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";

    [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = "";

    [Display(Name = "Display name (optional)")]
    public string? DisplayName { get; set; }
}
