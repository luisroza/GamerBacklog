namespace GameBacklog.Domain;

public sealed record GameEntry(
    int Id,
    string Title,
    string Platform,
    GameStatus Status,
    OwnershipType Ownership,
    int Priority,
    int ProgressPercent,
    double Rating,
    int EstimatedHours,
    int PlayedHours,
    string CoverGradient,
    string[] Genres,
    string[] Tags,
    string Note,
    DateTime AddedOn,
    DateTime? FinishedOn = null)
{
    public string StatusLabel => Status switch
    {
        GameStatus.WantToPlay => "Want to Play",
        _ => Status.ToString()
    };

    public string OwnershipLabel => Ownership.ToString();
}
