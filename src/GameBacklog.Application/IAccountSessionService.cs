namespace GameBacklog.Application;

public interface IAccountSessionService
{
    UserAccount? CurrentUser { get; }
    bool IsAuthenticated { get; }
    bool Register(string username, string password, out string message);
    bool Login(string username, string password, out string message);
    void Logout();
}
