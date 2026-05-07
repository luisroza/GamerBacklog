using GameBacklog.Domain;

namespace GameBacklog.Application;

public sealed record ManualGameRequest(string Title, string Platform, GameStatus Status, OwnershipType Ownership, int EstimatedHours, string Notes);
