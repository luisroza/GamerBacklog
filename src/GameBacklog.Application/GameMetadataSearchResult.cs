namespace GameBacklog.Application;

public sealed record GameMetadataSearchResult(string Provider, string ProviderGameId, string Title, string[] Platforms, string[] Genres, int? ReleaseYear, string? CoverUrl, string Summary);
