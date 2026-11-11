using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Cicalany.Wpf.Models;

namespace Cicalany.Wpf.Services;

/// <summary>Az adatokat az API tárolja; csak a még le nem adott kosár marad helyben.</summary>
public sealed class ApiDataService : IDataService
{
    private sealed record CartRec(int Id, int UserId, CatGirlDto Cat);
    private readonly List<CartRec> _cart = new();
    private readonly HttpClient _client;
    private readonly ApiAuthService _authentication;
    private int _nextCartId = 1;

    public ApiDataService(HttpClient? client = null)
    {
        _client = client ?? ApiAuthService.DefaultClient;
        _authentication = new ApiAuthService(_client);
    }

    public Task<LoginResult> RegisterAsync(string userName, string password) =>
        _authentication.RegisterAsync(userName, password);

    public Task<LoginResult> LoginAsync(string userName, string password) =>
        _authentication.LoginAsync(userName, password);

    public async Task<List<CatGirlDto>> GetCatGirlsAsync()
    {
        var cats = await SendAsync<List<CatGirlDto>>(HttpMethod.Get, "api/catgirls");
        foreach (var cat in cats) SetImageAddress(cat);
        return cats;
    }

    public async Task<CatGirlDto> CreateCatGirlAsync(CatGirlInput input)
    {
        using var form = new MultipartFormDataContent();
        form.Add(new StringContent(JsonSerializer.Serialize(new
        {
            input.Name, input.Age, input.HeightCm, input.WeightKg,
            input.Phone, input.Email, input.Description
        })), "details");
        if (string.IsNullOrWhiteSpace(input.ImagePath))
            throw new InvalidOperationException("A fénykép feltöltése kötelező.");
        try
        {
            form.Add(new StreamContent(File.OpenRead(input.ImagePath)), "image", Path.GetFileName(input.ImagePath));
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        {
            throw new InvalidOperationException("A kiválasztott kép nem olvasható. Válaszd ki újra.", error);
        }
        var cat = await SendAsync<CatGirlDto>(HttpMethod.Post, "api/catgirls/with-image", form);
        SetImageAddress(cat);
        return cat;
    }

    public Task<List<CartItemDto>> GetCartAsync(int userId) =>
        Task.FromResult(_cart.Where(x => x.UserId == userId).Select(ToDto).ToList());

    public async Task<CartItemDto> AddToCartAsync(int userId, int catGirlId)
    {
        var cat = await SendAsync<CatGirlDto>(HttpMethod.Get, $"api/catgirls/{catGirlId}");
        if (_cart.Any(x => x.UserId == userId && x.Cat.Id == catGirlId))
            throw new InvalidOperationException("Ez a cicálány már a kosárban van.");
        var item = new CartRec(_nextCartId++, userId, cat);
        _cart.Add(item);
        return ToDto(item);
    }

    public Task RemoveFromCartAsync(int userId, int cartItemId)
    {
        var item = _cart.FirstOrDefault(x => x.Id == cartItemId && x.UserId == userId)
            ?? throw new InvalidOperationException("Nem található a kosártétel.");
        _cart.Remove(item);
        return Task.CompletedTask;
    }

    public Task<List<OrderDto>> GetOrdersAsync(int userId) =>
        SendAsync<List<OrderDto>>(HttpMethod.Get, $"api/orders/user/{userId}");

    public async Task<OrderDto> PlaceOrderAsync(int userId, DateTime date, string location)
    {
        var items = _cart.Where(x => x.UserId == userId).ToList();
        if (items.Count == 0) throw new InvalidOperationException("Üres a kosár.");
        var order = await SendAsync<OrderDto>(HttpMethod.Post, "api/orders", JsonContent.Create(new
        {
            userId, orderDate = date, location,
            catGirlIds = items.Select(x => x.Cat.Id).ToList()
        }));
        // Csak sikeres mentés után ürítjük a leadott tételeket.
        _cart.RemoveAll(items.Contains);
        return order;
    }

    private void SetImageAddress(CatGirlDto cat)
    {
        if (!string.IsNullOrWhiteSpace(cat.ImagePath))
            cat.ImagePath = new Uri(_client.BaseAddress!, cat.ImagePath).AbsoluteUri;
    }

    private async Task<T> SendAsync<T>(HttpMethod method, string path, HttpContent? content = null)
    {
        using var request = new HttpRequestMessage(method, path) { Content = content };
        try
        {
            using var response = await _client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException(await ApiAuthService.ReadErrorAsync(response));
            return await response.Content.ReadFromJsonAsync<T>()
                ?? throw new InvalidOperationException("Az API üres választ küldött.");
        }
        catch (HttpRequestException error)
        {
            throw new InvalidOperationException("Nem érhető el az API. Ellenőrizd, hogy az Alkalmazas API fut-e.", error);
        }
        catch (TaskCanceledException error)
        {
            throw new InvalidOperationException("Az API nem válaszolt időben. Próbáld újra.", error);
        }
        catch (JsonException error)
        {
            throw new InvalidOperationException("Az API válasza nem olvasható.", error);
        }
    }

    private static CartItemDto ToDto(CartRec item) => new()
    {
        Id = item.Id, CatGirlId = item.Cat.Id, Name = item.Cat.Name, Age = item.Cat.Age,
        HeightCm = item.Cat.HeightCm, WeightKg = item.Cat.WeightKg,
        Price = item.Cat.Price, DurationHours = item.Cat.DurationHours
    };
}
