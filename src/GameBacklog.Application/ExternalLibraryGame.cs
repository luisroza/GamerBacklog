namespace GameBacklog.Application;

public sealed record ExternalLibraryGame(string Provider, string ProviderGameId, string Title, string Platform, int PlayedMinutes, DateTime? LastPlayedOn);
