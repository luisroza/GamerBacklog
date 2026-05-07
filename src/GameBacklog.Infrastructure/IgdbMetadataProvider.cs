using System.Net.Http.Headers;
using System.Text.Json;
using GameBacklog.Application;
using Microsoft.Extensions.Configuration;

namespace GameBacklog.Infrastructure;

public sealed class IgdbMetadataProvider(IHttpClientFactory httpClientFactory, IConfiguration configuration) : IGameMetadataProvider
{
    private string? _accessToken;
    private DateTimeOffset _accessTokenExpiresAt;
    public string Name => "IGDB";

    public async Task<IReadOnlyList<GameMetadataSearchResult>> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        var clientId = configuration["Igdb:ClientId"];
        var clientSecret = configuration["Igdb:ClientSecret"];
        if (string.IsNullOrWhiteSpace(query) || string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret)) return [];
        var token = await GetAccessTokenAsync(clientId, clientSecret, cancellationToken);
        var client = httpClientFactory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.igdb.com/v4/games");
        request.Headers.Add("Client-ID", clientId);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = new StringContent(BuildSearchQuery(query));
        using var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var games = await JsonSerializer.DeserializeAsync<List<IgdbGame>>(stream, JsonOptions, cancellationToken) ?? [];
        return games.Select(Map).ToList();
    }

    private async Task<string> GetAccessTokenAsync(string clientId, string clientSecret, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(_accessToken) && _accessTokenExpiresAt > DateTimeOffset.UtcNow.AddMinutes(5)) return _accessToken;
        var client = httpClientFactory.CreateClient();
        var url = $"https://id.twitch.tv/oauth2/token?client_id={Uri.EscapeDataString(clientId)}&client_secret={Uri.EscapeDataString(clientSecret)}&grant_type=client_credentials";
        using var response = await client.PostAsync(url, null, cancellationToken);
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var token = await JsonSerializer.DeserializeAsync<IgdbToken>(stream, JsonOptions, cancellationToken) ?? throw new InvalidOperationException("IGDB token response was empty.");
        _accessToken = token.AccessToken;
        _accessTokenExpiresAt = DateTimeOffset.UtcNow.AddSeconds(token.ExpiresIn);
        return _accessToken;
    }

    private static string BuildSearchQuery(string query) { var escaped = query.Replace("\\", "\\\\").Replace("\"", "\\\""); return $"search \"{escaped}\"; fields name,summary,first_release_date,cover.image_id,genres.name,platforms.name,total_rating; where version_parent = null; limit 10;"; }
    private static GameMetadataSearchResult Map(IgdbGame game) { var platforms = game.Platforms?.Select(p => p.Name).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray() ?? []; var genres = game.Genres?.Select(g => g.Name).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray() ?? []; int? year = game.FirstReleaseDate is null ? null : DateTimeOffset.FromUnixTimeSeconds(game.FirstReleaseDate.Value).Year; var cover = string.IsNullOrWhiteSpace(game.Cover?.ImageId) ? null : $"https://images.igdb.com/igdb/image/upload/t_cover_big/{game.Cover.ImageId}.jpg"; return new GameMetadataSearchResult("IGDB", game.Id.ToString(), game.Name ?? "Unknown game", platforms, genres, year, cover, game.Summary ?? "Imported from IGDB."); }
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true, PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };
    private sealed record IgdbToken(string AccessToken, int ExpiresIn, string TokenType);
    private sealed record IgdbGame(int Id, string? Name, string? Summary, long? FirstReleaseDate, IgdbCover? Cover, IgdbNamedValue[]? Genres, IgdbNamedValue[]? Platforms, double? TotalRating);
    private sealed record IgdbCover(string? ImageId);
    private sealed record IgdbNamedValue(string Name);
}
