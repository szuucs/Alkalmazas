using System.Security.Cryptography;
using Cicalany.Wpf.Models;

namespace Cicalany.Wpf.Services;

/// <summary>API nélküli adatforrás: minden a memóriában él, bezáráskor elvész.</summary>
public sealed class LocalDataService : IDataService
{
    private sealed record UserRec(int Id, string UserName, string PasswordHash);
    private sealed record CartRec(int Id, int UserId, int CatGirlId);
    private sealed record OrderRec(int Id, int UserId, DateTime Date, string Location, List<OrderLineDto> Lines);

    private readonly List<UserRec> _users = new();
    private readonly List<CatGirlDto> _catGirls = new();
    private readonly List<CartRec> _cart = new();
    private readonly List<OrderRec> _orders = new();
    private int _nextUserId = 1, _nextCartId = 1, _nextOrderId = 1;

    public LocalDataService()
    {
        AddUser("demo", "demo123");

        AddCatGirl("Mirci", 22, 163, 52);
        AddCatGirl("Szellő", 24, 170, 56);
        AddCatGirl("Pamacs", 19, 158, 48);
        AddCatGirl("Bandita", 27, 172, 61);
        AddCatGirl("Kozmosz", 25, 168, 54);
        AddCatGirl("Cirmi", 21, 160, 50);
        AddCatGirl("Hópehely", 23, 165, 51);
        AddCatGirl("Luna", 20, 161, 49);
    }

    // ---------- hitelesítés ----------

    public Task<LoginResult> RegisterAsync(string userName, string password)
    {
        var name = userName?.Trim() ?? "";
        if (name.Length is < 3 or > 64 || password is null || password.Length < 6)
            throw new InvalidOperationException("A név legalább 3, a jelszó legalább 6 karakter legyen.");
        if (_users.Any(u => string.Equals(u.UserName, name, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("Ez a felhasználónév foglalt.");
        return Task.FromResult(ToResult(AddUser(name, password)));
    }

    public Task<LoginResult> LoginAsync(string userName, string password)
    {
        var name = userName?.Trim() ?? "";
        var user = _users.FirstOrDefault(u => string.Equals(u.UserName, name, StringComparison.OrdinalIgnoreCase));
        if (user is null || !VerifyPassword(password ?? "", user.PasswordHash))
            throw new InvalidOperationException("Hibás felhasználónév vagy jelszó.");
        return Task.FromResult(ToResult(user));
    }

    // ---------- katalógus ----------

    public Task<List<CatGirlDto>> GetCatGirlsAsync() =>
        Task.FromResult(_catGirls.OrderBy(c => c.Name, StringComparer.CurrentCultureIgnoreCase).ToList());

    // ---------- kosár ----------

    public Task<List<CartItemDto>> GetCartAsync(int userId) =>
        Task.FromResult(_cart.Where(x => x.UserId == userId).OrderBy(x => x.Id).Select(ToDto).ToList());

    public Task<CartItemDto> AddToCartAsync(int userId, int catGirlId)
    {
        if (_catGirls.All(c => c.Id != catGirlId))
            throw new InvalidOperationException("Nem található a cicálány.");
        if (_cart.Any(x => x.UserId == userId && x.CatGirlId == catGirlId))
            throw new InvalidOperationException("Ez a cicálány már a kosárban van.");
        var item = new CartRec(_nextCartId++, userId, catGirlId);
        _cart.Add(item);
        return Task.FromResult(ToDto(item));
    }

    public Task RemoveFromCartAsync(int userId, int cartItemId)
    {
        var item = _cart.FirstOrDefault(x => x.Id == cartItemId && x.UserId == userId)
            ?? throw new InvalidOperationException("Nem található a kosártétel.");
        _cart.Remove(item);
        return Task.CompletedTask;
    }

    // ---------- rendelés ----------

    public Task<OrderDto> PlaceOrderAsync(int userId, DateTime date, string location)
    {
        var place = string.IsNullOrWhiteSpace(location) ? "Online" : location.Trim();
        var day = date.Date;
        if (day < DateTime.Today) throw new InvalidOperationException("A rendelés dátuma nem lehet korábbi a mainál.");
        if (place.Length > 200) throw new InvalidOperationException("A helyszín legfeljebb 200 karakter lehet.");

        var items = _cart.Where(x => x.UserId == userId).OrderBy(x => x.Id).ToList();
        if (items.Count == 0) throw new InvalidOperationException("Üres a kosár.");

        // a rendelés a rendeléskori adatokat őrzi meg
        var lines = items.Select(x =>
        {
            var cat = _catGirls.First(c => c.Id == x.CatGirlId);
            return new OrderLineDto
            {
                CatGirlId = cat.Id, Name = cat.Name, Age = cat.Age,
                HeightCm = cat.HeightCm, WeightKg = cat.WeightKg
            };
        }).ToList();

        var order = new OrderRec(_nextOrderId++, userId, day, place, lines);
        _orders.Add(order);
        _cart.RemoveAll(x => x.UserId == userId);
        return Task.FromResult(new OrderDto { Id = order.Id, OrderDate = day, Location = place, Lines = lines });
    }

    // ---------- segédek ----------

    private UserRec AddUser(string name, string password)
    {
        var user = new UserRec(_nextUserId++, name, HashPassword(password));
        _users.Add(user);
        return user;
    }

    private void AddCatGirl(string name, int age, double height, double weight) =>
        _catGirls.Add(new CatGirlDto
        {
            Id = _catGirls.Count + 1, Name = name, Age = age,
            HeightCm = height, WeightKg = weight
        });

    private static LoginResult ToResult(UserRec user) =>
        new() { UserName = user.UserName, UserId = user.Id };

    private CartItemDto ToDto(CartRec item)
    {
        var cat = _catGirls.First(c => c.Id == item.CatGirlId);
        return new CartItemDto
        {
            Id = item.Id, CatGirlId = cat.Id, Name = cat.Name, Age = cat.Age,
            HeightCm = cat.HeightCm, WeightKg = cat.WeightKg
        };
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
