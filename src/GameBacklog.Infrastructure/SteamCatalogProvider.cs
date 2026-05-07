using System.Text.Json;
using System.Text.Json.Serialization;
using GameBacklog.Application;

namespace GameBacklog.Infrastructure;

public sealed class SteamCatalogProvider(IHttpClientFactory httpClientFactory) : IGameCatalogProvider
{
    private IReadOnlyList<SteamApp>? _cachedApps;
    private DateTimeOffset _cacheExpiresAt;
    private static readonly IReadOnlyList<SteamApp> FallbackApps = [new(367520, "Hollow Knight"), new(1245620, "ELDEN RING"), new(1145360, "Hades"), new(1086940, "Baldur's Gate 3"), new(413150, "Stardew Valley"), new(2379780, "Balatro")];
    public string SourceName => "Valve Steam";
    public string Description => "Searches the Steam app catalog through Valve's public app list endpoint.";

    public async Task<IReadOnlyList<GameMetadataSearchResult>> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query)) return [];
        var apps = await GetAppsAsync(cancellationToken);
        return apps.Where(app => !string.IsNullOrWhiteSpace(app.Name)).Where(app => app.Name.Contains(query.Trim(), StringComparison.OrdinalIgnoreCase)).OrderBy(app => app.Name.Length).ThenBy(app => app.Name).Take(12).Select(Map).ToList();
    }

    private async Task<IReadOnlyList<SteamApp>> GetAppsAsync(CancellationToken cancellationToken)
    {
        if (_cachedApps is not null && _cacheExpiresAt > DateTimeOffset.UtcNow) return _cachedApps;
        try
        {
            var client = httpClientFactory.CreateClient();
            using var response = await client.GetAsync("https://api.steampowered.com/ISteamApps/GetAppList/v2/", cancellationToken);
            response.EnsureSuccessStatusCode();
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var payload = await JsonSerializer.DeserializeAsync<SteamAppListResponse>(stream, JsonOptions, cancellationToken);
            _cachedApps = payload?.AppList?.Apps?.Where(app => !string.IsNullOrWhiteSpace(app.Name)).ToList() ?? FallbackApps;
            _cacheExpiresAt = DateTimeOffset.UtcNow.AddHours(12);
        }
        catch
        {
            _cachedApps = FallbackApps;
            _cacheExpiresAt = DateTimeOffset.UtcNow.AddMinutes(15);
        }
        return _cachedApps;
    }

    private static GameMetadataSearchResult Map(SteamApp app) => new("Valve Steam", app.AppId.ToString(), app.Name, ["Steam", "PC"], ["Steam catalog"], null, $"https://shared.akamai.steamstatic.com/store_item_assets/steam/apps/{app.AppId}/header.jpg", "Registered in the Valve Steam application catalog. Add it to your backlog, then enrich metadata through IGDB/RAWG later.");
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true, PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };
    private sealed record SteamAppListResponse(SteamAppList? AppList);
    private sealed record SteamAppList(SteamApp[]? Apps);
    private sealed record SteamApp([property: JsonPropertyName("appid")] int AppId, string Name);
}
