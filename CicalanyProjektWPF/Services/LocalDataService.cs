using System.Security.Cryptography;
using Cicalany.Wpf.Models;

namespace Cicalany.Wpf.Services;

/// <summary>API nélküli adatforrás: a felhasználók a memóriában élnek, bezáráskor elvesznek.</summary>
public sealed class LocalDataService : IDataService
{
    private sealed record UserRec(int Id, string UserName, string PasswordHash);

    private readonly List<UserRec> _users = new();
    private readonly Dictionary<string, int> _sessions = new();
    private int _nextUserId = 1;
    private int _currentUserId;

    public LocalDataService()
    {
        AddUser("demo", "demo123");
    }

    public void SetToken(string? token) =>
        _currentUserId = token is not null && _sessions.TryGetValue(token, out var id) ? id : 0;

    public Task<LoginResult> RegisterAsync(string userName, string password)
    {
        var name = userName?.Trim() ?? "";
        if (name.Length is < 3 or > 64 || password is null || password.Length < 6)
            throw new InvalidOperationException("A név legalább 3, a jelszó legalább 6 karakter legyen.");
        if (_users.Any(u => string.Equals(u.UserName, name, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("Ez a felhasználónév foglalt.");
        return Task.FromResult(NewSession(AddUser(name, password)));
    }

    public Task<LoginResult> LoginAsync(string userName, string password)
    {
        var name = userName?.Trim() ?? "";
        var user = _users.FirstOrDefault(u => string.Equals(u.UserName, name, StringComparison.OrdinalIgnoreCase));
        if (user is null || !VerifyPassword(password ?? "", user.PasswordHash))
            throw new InvalidOperationException("Hibás felhasználónév vagy jelszó.");
        return Task.FromResult(NewSession(user));
    }

    private UserRec AddUser(string name, string password)
    {
        var user = new UserRec(_nextUserId++, name, HashPassword(password));
        _users.Add(user);
        return user;
    }

    private LoginResult NewSession(UserRec user)
    {
        var token = Guid.NewGuid().ToString("N");
        _sessions[token] = user.Id;
        return new LoginResult { Token = token, UserName = user.UserName, UserId = user.Id };
    }

    private static string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100_000, HashAlgorithmName.SHA256, 32);
        return Convert.ToBase64String(salt) + "." + Convert.ToBase64String(hash);
    }

    private static bool VerifyPassword(string password, string stored)
    {
        var parts = stored.Split('.');
        if (parts.Length != 2) return false;
        var salt = Convert.FromBase64String(parts[0]);
        var correct = Convert.FromBase64String(parts[1]);
        var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100_000, HashAlgorithmName.SHA256, correct.Length);
        return CryptographicOperations.FixedTimeEquals(correct, actual);
    }
}
