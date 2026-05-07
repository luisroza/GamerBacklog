using System.Security.Cryptography;
using System.Text;
using GameBacklog.Application;

namespace GameBacklog.Infrastructure;

public sealed class InMemoryAccountSessionService : IAccountSessionService
{
    private static readonly object Sync = new();
    private static readonly Dictionary<string, string> PasswordHashes = new(StringComparer.OrdinalIgnoreCase);
    private UserAccount? _currentUser;
    public UserAccount? CurrentUser => _currentUser;
    public bool IsAuthenticated => _currentUser is not null;

    public bool Register(string username, string password, out string message)
    {
        username = username.Trim();
        if (username.Length < 3) { message = "Username must be at least 3 characters."; return false; }
        if (password.Length < 6) { message = "Password must be at least 6 characters."; return false; }
        lock (Sync)
        {
            if (PasswordHashes.ContainsKey(username)) { message = "That username already exists."; return false; }
            PasswordHashes[username] = Hash(password);
        }
        _currentUser = new UserAccount(username, DateTime.Now);
        message = $"Account created. Welcome, {username}.";
        return true;
    }

    public bool Login(string username, string password, out string message)
    {
        username = username.Trim();
        lock (Sync)
        {
            if (!PasswordHashes.TryGetValue(username, out var hash) || hash != Hash(password)) { message = "Invalid username or password."; return false; }
        }
        _currentUser = new UserAccount(username, DateTime.Now);
        message = $"Signed in as {username}.";
        return true;
    }

    public void Logout() => _currentUser = null;
    private static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
}
