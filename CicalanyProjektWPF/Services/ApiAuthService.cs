using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Cicalany.Wpf.Models;

namespace Cicalany.Wpf.Services;

/// <summary>A regisztrációt és a bejelentkezést a UsersController végzi az API-n.</summary>
public sealed class ApiAuthService
{
    // Az API http indítási profiljának címe. A kliens több kéréshez is újrahasználható.
    private static readonly HttpClient DefaultClient = new()
    {
        BaseAddress = new Uri("http://localhost:5077/"),
        Timeout = TimeSpan.FromSeconds(15)
    };

    private readonly HttpClient _client;

    public ApiAuthService(HttpClient? client = null) => _client = client ?? DefaultClient;

    public Task<LoginResult> RegisterAsync(string userName, string password) =>
        AuthenticateAsync("api/users", userName, password);

    public Task<LoginResult> LoginAsync(string userName, string password) =>
        AuthenticateAsync("api/users/login", userName, password);

    private async Task<LoginResult> AuthenticateAsync(string path, string userName, string password)
    {
        try
        {
            using var response = await _client.PostAsJsonAsync(path,
                new { name = userName.Trim(), password });

            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException(await ReadErrorAsync(response));

            var user = await response.Content.ReadFromJsonAsync<UserResponse>();
            if (user is null || user.Id <= 0 || string.IsNullOrWhiteSpace(user.Name))
                throw new InvalidOperationException("Az API érvénytelen felhasználói adatokat küldött.");

            return new LoginResult { UserId = user.Id, UserName = user.Name };
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException(
                "Nem érhető el az API. Ellenőrizd, hogy az Alkalmazas API fut-e.", ex);
        }
        catch (TaskCanceledException ex)
        {
            throw new InvalidOperationException("Az API nem válaszolt időben. Próbáld újra.", ex);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Az API válasza nem olvasható.", ex);
        }
    }

    private static async Task<string> ReadErrorAsync(HttpResponseMessage response)
    {
        var fallback = $"Az API elutasította a kérést (HTTP {(int)response.StatusCode}).";
        var body = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(body)) return fallback;

        // A controller szöveges hibát, az ASP.NET validáció ProblemDetails JSON-t küld.
        if (response.Content.Headers.ContentType?.MediaType == "text/plain") return body;

        try
        {
            using var document = JsonDocument.Parse(body);
            var root = document.RootElement;
            if (root.ValueKind == JsonValueKind.String) return root.GetString() ?? fallback;
            if (root.ValueKind == JsonValueKind.Object)
            {
                if (root.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
                {
                    var messages = errors.EnumerateObject()
                        .Where(e => e.Value.ValueKind == JsonValueKind.Array)
                        .SelectMany(e => e.Value.EnumerateArray())
                        .Where(e => e.ValueKind == JsonValueKind.String)
                        .Select(e => e.GetString());
                    var message = string.Join(" ", messages);
                    if (!string.IsNullOrWhiteSpace(message)) return message;
                }
                if (root.TryGetProperty("title", out var title) && title.ValueKind == JsonValueKind.String)
                    return title.GetString() ?? fallback;
            }
        }
        catch (JsonException)
        {
            // Például HTML hibaoldal esetén is rövid, érthető üzenet jelenjen meg.
        }

        return fallback;
    }

    private sealed record UserResponse(int Id, string Name);
}
