namespace GamerBacklog.Domain.Enums;

public static class StatusInfo
{
    public static string Label(this PlayStatus s) => s switch
    {
        PlayStatus.Backlog => "Want to Play",
        PlayStatus.Playing => "Playing",
        PlayStatus.Played => "Played",
        PlayStatus.Abandoned => "Abandoned",
        PlayStatus.Wishlist => "Wishlist",
        _ => "—"
    };

    public static string HexColor(this PlayStatus s) => s switch
    {
        PlayStatus.Backlog => "#8B5CF6",
        PlayStatus.Playing => "#38BDF8",
        PlayStatus.Played => "#34D399",
        PlayStatus.Abandoned => "#F87171",
        PlayStatus.Wishlist => "#FBBF24",
        _ => "#9CA3AF"
    };

    public static string PillClass(this PlayStatus s) => s switch
    {
        PlayStatus.Backlog => "pill pill-backlog",
        PlayStatus.Playing => "pill pill-playing",
        PlayStatus.Played => "pill pill-played",
        PlayStatus.Abandoned => "pill pill-abandoned",
        PlayStatus.Wishlist => "pill pill-wishlist",
        _ => "pill"
    };
}
