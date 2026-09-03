using System.Text.Json;
using System.Text.Json.Serialization;
using GamerBacklog.Data;
using GamerBacklog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GamerBacklog.Integrations;

public class SteamClient : ISteamClient
{
    private readonly IConfiguration _config;
    private readonly AppDbContext _db;
    private readonly IHttpClientFactory _httpFactory;
    private readonly ILogger<SteamClient> _logger;

    public SteamClient(IConfiguration config, AppDbContext db, IHttpClientFactory httpFactory, ILogger<SteamClient> logger)
    {
        _config = config;
        _db = db;
        _httpFactory = httpFactory;
        _logger = logger;
    }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_config["Steam:ApiKey"]);

    public async Task<SteamSyncResult> SyncAchievementsAsync(ApplicationUser user)
    {
        if (IsConfigured && !string.IsNullOrWhiteSpace(user.SteamId))
        {
            try
            {
                return await SyncRealAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Real Steam sync failed. Falling back to simulated mode.");
            }
        }

        return await SimulateAsync(user);
    }

    // ---------------- Real (Steam Web API) ----------------

    private async Task<SteamSyncResult> SyncRealAsync(ApplicationUser user)
    {
        var key = _config["Steam:ApiKey"]!;
        var steamId = user.SteamId!;
        var http = _httpFactory.CreateClient();

        var owned = await http.GetFromJsonAsync<OwnedGamesResponse>(
            $"https://api.steampowered.com/IPlayerService/GetOwnedGames/v1/?key={key}&steamid={steamId}&include_appinfo=true&include_played_free_games=true");

        var ownedGames = owned?.Response?.Games ?? new List<OwnedGame>();
        var allDbGames = await _db.Games.ToListAsync();

        int newAchievements = 0;
        int gamesTouched = 0;
        var processed = 0;

        foreach (var og in ownedGames)
        {
            if (string.IsNullOrWhiteSpace(og.Name) || processed >= 10) continue;

            var dbGame = allDbGames.FirstOrDefault(g => g.Name.Equals(og.Name.Trim(), StringComparison.OrdinalIgnoreCase));
            if (dbGame == null) continue;

            dbGame.SteamAppId = og.AppId;
            processed++;

            SchemaResponse? schema = null;
            PlayerAchievementsResponse? player = null;
            try
            {
                schema = await http.GetFromJsonAsync<SchemaResponse>(
                    $"https://api.steampowered.com/ISteamUserStats/GetSchemaForGame/v2/?key={key}&appid={og.AppId}");
                player = await http.GetFromJsonAsync<PlayerAchievementsResponse>(
                    $"https://api.steampowered.com/ISteamUserStats/GetPlayerAchievements/v1/?key={key}&steamid={steamId}&appid={og.AppId}");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch Steam achievements for {Game}", dbGame.Name);
                continue;
            }

            var schemaAchievements = schema?.Game?.AvailableGameStats ?? new List<SchemaAchievement>();
            var playerAchievements = player?.PlayerStats?.Achievements ?? new List<SteamAchievement>();
            var playerByApi = playerAchievements.ToDictionary(a => a.APIName ?? "", a => a, StringComparer.OrdinalIgnoreCase);

            foreach (var sa in schemaAchievements)
            {
                if (string.IsNullOrWhiteSpace(sa.Name)) continue;

                var achievement = await _db.Achievements.FirstOrDefaultAsync(a =>
                    a.GameId == dbGame.Id && a.ExternalId == $"{og.AppId}-{sa.Name}");

                if (achievement == null)
                {
                    achievement = new Achievement
                    {
                        GameId = dbGame.Id,
                        ExternalId = $"{og.AppId}-{sa.Name}",
                        Name = string.IsNullOrWhiteSpace(sa.DisplayName) ? sa.Name : sa.DisplayName,
                        Description = sa.Description ?? "",
                        IconUrl = sa.Icon,
                        RarityPercent = 50
                    };
                    _db.Achievements.Add(achievement);
                    await _db.SaveChangesAsync();
                }

                if (playerByApi.TryGetValue(sa.Name, out var pa) && pa.Achieved == 1)
                {
                    var has = await _db.UserAchievements.AnyAsync(ua => ua.UserId == user.Id && ua.AchievementId == achievement.Id);
                    if (!has)
                    {
                        var unlockedAt = pa.UnlockTime > 0
                            ? DateTimeOffset.FromUnixTimeSeconds(pa.UnlockTime).UtcDateTime
                            : DateTime.UtcNow;

                        _db.UserAchievements.Add(new UserAchievement
                        {
                            UserId = user.Id,
                            AchievementId = achievement.Id,
                            UnlockedAt = unlockedAt
                        });
                        newAchievements++;
                    }
                }
            }

            gamesTouched++;
        }

        user.LastSteamSyncAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return new SteamSyncResult(newAchievements, gamesTouched, false,
            $"Real sync completed: {newAchievements} new achievement{(newAchievements == 1 ? "" : "s")} across {gamesTouched} game{(gamesTouched == 1 ? "" : "s")}.");
    }

    // ---------------- Simulado ----------------

    private async Task<SteamSyncResult> SimulateAsync(ApplicationUser user)
    {
        var userGames = await _db.UserGames
            .Include(ug => ug.Game)
            .ThenInclude(g => g.Achievements)
            .Where(ug => ug.UserId == user.Id)
            .ToListAsync();

        int newUnlocked = 0;
        int gamesTouched = 0;
        var rnd = new Random();
        var displayName = string.IsNullOrWhiteSpace(user.DisplayName) ? user.UserName : user.DisplayName;

        foreach (var ug in userGames)
        {
            var achievements = ug.Game.Achievements.OrderBy(a => a.Id).ToList();
            if (achievements.Count == 0) continue;

            var ids = achievements.Select(a => a.Id).ToList();
            var unlockedIds = await _db.UserAchievements
                .Where(ua => ua.UserId == user.Id && ids.Contains(ua.AchievementId))
                .Select(ua => ua.AchievementId)
                .ToListAsync();

            var targetPercent = rnd.Next(30, 91);
            var target = (int)Math.Ceiling(achievements.Count * targetPercent / 100.0);
            var missing = achievements.Where(a => !unlockedIds.Contains(a.Id)).OrderBy(a => a.RarityPercent).ToList();
            var need = Math.Max(0, target - unlockedIds.Count);

            var gained = 0;
            foreach (var a in missing.Take(need))
            {
                _db.UserAchievements.Add(new UserAchievement
                {
                    UserId = user.Id,
                    AchievementId = a.Id,
                    UnlockedAt = DateTime.UtcNow.AddDays(-rnd.Next(1, 200))
                });
                newUnlocked++;
                gained++;
            }

            if (gained > 0)
            {
                gamesTouched++;
                if (gamesTouched <= 3)
                {
                    _db.Activities.Add(new Domain.Entities.Activity
                    {
                        UserId = user.Id,
                        GameId = ug.GameId,
                        Type = Domain.Enums.ActivityType.Achievement,
                        Text = $"{displayName} unlocked {gained} achievement{(gained == 1 ? "" : "s")} in {ug.Game.Name} via Steam.",
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
        }

        user.LastSteamSyncAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return new SteamSyncResult(newUnlocked, gamesTouched, true,
            $"Sync completed (simulated mode): {newUnlocked} new achievement{(newUnlocked == 1 ? "" : "s")} across {gamesTouched} game{(gamesTouched == 1 ? "" : "s")}.");
    }

    // ---------------- DTOs da Steam Web API ----------------

    private sealed class OwnedGamesResponse
    {
        [JsonPropertyName("response")]
        public OwnedGamesResponseData? Response { get; set; }
    }

    private sealed class OwnedGamesResponseData
    {
        [JsonPropertyName("game_count")]
        public int GameCount { get; set; }

        [JsonPropertyName("games")]
        public List<OwnedGame>? Games { get; set; }
    }

    private sealed class OwnedGame
    {
        [JsonPropertyName("appid")]
        public int AppId { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }

    private sealed class PlayerAchievementsResponse
    {
        [JsonPropertyName("playerstats")]
        public PlayerStats? PlayerStats { get; set; }
    }

    private sealed class PlayerStats
    {
        [JsonPropertyName("steamID")]
        public string? SteamID { get; set; }

        [JsonPropertyName("gameName")]
        public string? GameName { get; set; }

        [JsonPropertyName("achievements")]
        public List<SteamAchievement>? Achievements { get; set; }
    }

    private sealed class SteamAchievement
    {
        [JsonPropertyName("apiname")]
        public string? APIName { get; set; }

        [JsonPropertyName("achieved")]
        public int Achieved { get; set; }

        [JsonPropertyName("unlocktime")]
        public long UnlockTime { get; set; }
    }

    private sealed class SchemaResponse
    {
        [JsonPropertyName("game")]
        public SchemaGame? Game { get; set; }
    }

    private sealed class SchemaGame
    {
        [JsonPropertyName("gameName")]
        public string? GameName { get; set; }

        [JsonPropertyName("availableGameStats")]
        public List<SchemaAchievement>? AvailableGameStats { get; set; }
    }

    private sealed class SchemaAchievement
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("displayName")]
        public string? DisplayName { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("icon")]
        public string? Icon { get; set; }
    }
}
