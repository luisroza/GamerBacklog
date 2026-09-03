using System.Net.Http.Json;
using System.Text.Json.Serialization;
using GamerBacklog.Data;
using GamerBacklog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GamerBacklog.Integrations;

/// <summary>
/// RAWG API client (https://rawg.io/apidocs) — free tier (~20k requests/month),
/// the only major free API that exposes each game's Metacritic score.
///
/// Persistence strategy (on demand, games are stored locally and never fetched twice):
/// - When a user searches for a game we don't have, ImportSearchResultsAsync queries
///   RAWG and persists the results into the local database — once per game.
/// - Per-game details (description/developer) are fetched lazily, at most ONCE per game,
///   when someone opens a game page whose Summary is still empty (HydrateGameAsync).
/// - The seed only pulls a small initial list (2 requests for 40 top games).
/// Every call is fault-tolerant: failures return empty results so callers keep working offline.
/// </summary>
public class RawgClient : ICatalogClient
{
    private readonly IConfiguration _config;
    private readonly AppDbContext _db;
    private readonly IHttpClientFactory _httpFactory;
    private readonly ILogger<RawgClient> _logger;

    public RawgClient(IConfiguration config, AppDbContext db, IHttpClientFactory httpFactory, ILogger<RawgClient> logger)
    {
        _config = config;
        _db = db;
        _httpFactory = httpFactory;
        _logger = logger;
    }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_config["Rawg:ApiKey"]);

    private string Key => _config["Rawg:ApiKey"]!;
    private HttpClient CreateClient() => _httpFactory.CreateClient("rawg");

    /// <summary>
    /// Small initial list for the seed (2 requests for ~40 top-rated games) —
    /// list pages carry metacritic/cover/genres/platforms, no per-game calls.
    /// </summary>
    public async Task<IReadOnlyList<Game>> GetPopularGamesAsync(int take = 40)
    {
        if (!IsConfigured) return new List<Game>();

        try
        {
            var http = CreateClient();
            var games = new List<Game>();
            var page = 1;

            while (games.Count < take && page <= 3)
            {
                var url = $"games?key={Key}&ordering=-metacritic&page_size=40&page={page}";
                var response = await http.GetFromJsonAsync<RawgListResponse>(url);
                var results = response?.Results;
                if (results == null || results.Count == 0) break;

                foreach (var r in results)
                {
                    if (string.IsNullOrWhiteSpace(r.Name)) continue;
                    games.Add(Map(r));
                }
                page++;
            }

            return games.Take(take).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "RAWG catalog fetch failed. Falling back to the demo catalog.");
            return new List<Game>();
        }
    }

    public async Task<Game?> GetByIdAsync(long rawgId)
    {
        if (!IsConfigured) return null;
        try
        {
            var http = CreateClient();
            var detail = await http.GetFromJsonAsync<RawgGame>($"games/{rawgId}?key={Key}");
            if (detail == null || string.IsNullOrWhiteSpace(detail.Name)) return null;
            var game = Map(detail);
            ApplyDetail(game, detail);
            return game;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "RAWG detail fetch failed for id {Id}", rawgId);
            return null;
        }
    }

    /// <summary>
    /// Called when a user searches for a game we don't have: queries the RAWG search
    /// endpoint and persists any new results into the local database (once per game).
    /// Returns how many new games were imported.
    /// </summary>
    public async Task<int> ImportSearchResultsAsync(string query, int take = 6)
    {
        if (!IsConfigured || string.IsNullOrWhiteSpace(query)) return 0;

        try
        {
            var http = CreateClient();
            var url = $"games?key={Key}&search={Uri.EscapeDataString(query.Trim())}&page_size={take}";
            var response = await http.GetFromJsonAsync<RawgListResponse>(url);
            var results = response?.Results;
            if (results == null || results.Count == 0) return 0;

            var mapped = results
                .Where(r => !string.IsNullOrWhiteSpace(r.Name))
                .Select(Map)
                .ToList();

            var (added, _) = await UpsertAsync(mapped);
            return added;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "RAWG search import failed for {Query}", query);
            return 0;
        }
    }

    /// <summary>
    /// Fills in description/developer for a game already stored locally.
    /// Guarded by "Summary is empty", so the detail endpoint is called at most once per game.
    /// </summary>
    public async Task<bool> HydrateGameAsync(Game game)
    {
        if (!IsConfigured || game.RawgId == null || !string.IsNullOrWhiteSpace(game.Summary)) return false;

        try
        {
            var http = CreateClient();
            var detail = await http.GetFromJsonAsync<RawgGame>($"games/{game.RawgId}?key={Key}");
            if (detail == null) return false;

            ApplyDetail(game, detail);
            await _db.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "RAWG detail hydration failed for {Game}", game.Name);
            return false;
        }
    }

    /// <summary>
    /// Upserts games into the local database, matching by RawgId first, then by
    /// normalized name (so demo/manual entries merge instead of duplicating).
    /// </summary>
    private async Task<(int Added, int Updated)> UpsertAsync(IReadOnlyList<Game> incoming)
    {
        var all = await _db.Games.ToListAsync();
        var byRawgId = all.Where(g => g.RawgId != null).ToDictionary(g => g.RawgId!.Value, g => g);
        var byName = new Dictionary<string, Game>(StringComparer.Ordinal);
        foreach (var g in all)
        {
            byName.TryAdd(Normalize(g.Name), g);
        }

        int added = 0, updated = 0;
        foreach (var inc in incoming)
        {
            Game? existing = null;
            if (inc.RawgId != null) byRawgId.TryGetValue(inc.RawgId.Value, out existing);
            existing ??= byName.GetValueOrDefault(Normalize(inc.Name));

            if (existing != null)
            {
                // Already stored: only fill gaps — never re-fetch what we already know.
                if (existing.RawgId == null && inc.RawgId != null) existing.RawgId = inc.RawgId;
                if (existing.Metacritic == null && inc.Metacritic != null) existing.Metacritic = inc.Metacritic;
                if (string.IsNullOrWhiteSpace(existing.ImageUrl) && !string.IsNullOrWhiteSpace(inc.ImageUrl)) existing.ImageUrl = inc.ImageUrl;
                if (existing.ReleaseYear == null) existing.ReleaseYear = inc.ReleaseYear;
                if (string.IsNullOrWhiteSpace(existing.Platforms) && !string.IsNullOrWhiteSpace(inc.Platforms)) existing.Platforms = inc.Platforms;
                if (string.IsNullOrWhiteSpace(existing.Genres) && !string.IsNullOrWhiteSpace(inc.Genres)) existing.Genres = inc.Genres;
                updated++;
            }
            else
            {
                _db.Games.Add(inc);
                if (inc.RawgId != null && !byRawgId.ContainsKey(inc.RawgId.Value)) byRawgId[inc.RawgId.Value] = inc;
                byName[Normalize(inc.Name)] = inc;
                added++;
            }
        }

        if (added > 0 || updated > 0)
        {
            await _db.SaveChangesAsync();
        }
        return (added, updated);
    }

    private static string Normalize(string name) => name.Trim().ToLowerInvariant();

    private static Game Map(RawgGame r)
    {
        var name = r.Name!.Trim();
        return new Game
        {
            RawgId = r.Id,
            Name = name,
            Metacritic = r.Metacritic,
            ImageUrl = string.IsNullOrWhiteSpace(r.BackgroundImage) ? null : r.BackgroundImage,
            CoverUrl = "/covers/" + Uri.EscapeDataString(name),
            ReleaseYear = DateTime.TryParse(r.Released, out var released) ? released.Year : null,
            Platforms = JoinNames(r.Platforms?.Select(p => p.Platform?.Name)),
            Genres = JoinNames(r.Genres?.Select(g => g.Name))
        };
    }

    private static void ApplyDetail(Game game, RawgGame detail)
    {
        if (!string.IsNullOrWhiteSpace(detail.DescriptionRaw))
        {
            game.Summary = detail.DescriptionRaw.Length > 1200
                ? detail.DescriptionRaw[..1200].TrimEnd() + "…"
                : detail.DescriptionRaw;
        }
        game.Developer = JoinNames(detail.Developers?.Select(d => d.Name));
    }

    private static string JoinNames(IEnumerable<string?>? names) =>
        string.Join(",", (names ?? Enumerable.Empty<string?>())
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Select(n => n!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase));

    // ---------------- RAWG API DTOs ----------------

    private sealed class RawgListResponse
    {
        [JsonPropertyName("count")]
        public int? Count { get; set; }

        [JsonPropertyName("results")]
        public List<RawgGame>? Results { get; set; }
    }

    private sealed class RawgGame
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("metacritic")]
        public int? Metacritic { get; set; }

        [JsonPropertyName("released")]
        public string? Released { get; set; }

        [JsonPropertyName("background_image")]
        public string? BackgroundImage { get; set; }

        [JsonPropertyName("description_raw")]
        public string? DescriptionRaw { get; set; }

        [JsonPropertyName("genres")]
        public List<RawgRef>? Genres { get; set; }

        [JsonPropertyName("platforms")]
        public List<RawgPlatformWrapper>? Platforms { get; set; }

        [JsonPropertyName("developers")]
        public List<RawgRef>? Developers { get; set; }
    }

    private sealed class RawgPlatformWrapper
    {
        [JsonPropertyName("platform")]
        public RawgRef? Platform { get; set; }
    }

    private sealed class RawgRef
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }
}
