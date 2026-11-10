using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Windows.Media.Imaging;
using Cicalany.Wpf.Models;
using Cicalany.Wpf.Services;

// A futó API-n ellenőriz, majd törli a saját tesztadatait.
using var http = new HttpClient { BaseAddress = new Uri(args.FirstOrDefault() ?? "http://localhost:5088/") };
var api = new ApiDataService(http);
var users = new List<int>();
var cats = new List<int>();
var orders = new List<int>();
var temporaryImage = Path.Combine(Path.GetTempPath(), "catgirl-check-" + Guid.NewGuid().ToString("N") + ".png");
var unsupportedFile = Path.ChangeExtension(temporaryImage, ".txt");
try
{
    CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("hu-HU");
    var catalog = await api.GetCatGirlsAsync();
    Check(catalog.Count >= 8, "Starter catalog missing.");
    foreach (var cat in catalog.Where(cat => cat.ImagePath != null))
    {
        var bytes = await http.GetByteArrayAsync(cat.ImagePath);
        using var image = new MemoryStream(bytes);
        Check(BitmapFrame.Create(image, BitmapCreateOptions.None, BitmapCacheOption.OnLoad).PixelWidth > 0, "Catalog image cannot be decoded.");
    }
    var name = "api_check_" + Guid.NewGuid().ToString("N");
    var login = await api.RegisterAsync(name, "CheckPassword123!");
    users.Add(login.UserId);
    Check((await api.LoginAsync(name, "CheckPassword123!")).UserId == login.UserId, "Login failed.");
    var other = await api.RegisterAsync(name + "_other", "CheckPassword123!");
    users.Add(other.UserId);

    var png = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+jRZkAAAAASUVORK5CYII=");
    File.WriteAllBytes(temporaryImage, png);
    var created = await api.CreateCatGirlAsync(Input("Uploaded check", 300, 500, temporaryImage));
    cats.Add(created.Id);
    File.Delete(temporaryImage);
    Check((await http.GetByteArrayAsync(created.ImagePath)).SequenceEqual(png), "Uploaded picture changed or was not saved.");
    var minimum = await api.CreateCatGirlAsync(Input("Minimum check", 0, 0));
    cats.Add(minimum.Id);
    Check(minimum.ImagePath is null, "Optional image should stay empty.");
    var fraction = await api.CreateCatGirlAsync(Input("Decimal check", 150.5, 60.5));
    cats.Add(fraction.Id);
    Check(fraction.HeightCm == 150.5 && fraction.WeightKg == 60.5, "Decimal values changed with Hungarian culture.");

    var newSession = new ApiDataService(http);
    Check((await newSession.GetCatGirlsAsync()).Any(cat => cat.Id == created.Id && cat.ImagePath == created.ImagePath), "Advertisement was not persisted.");
    foreach (var (height, weight) in new[] { (-0.1, 50d), (300.1, 50d), (150d, -0.1), (150d, 500.1) })
        await Reject(() => api.CreateCatGirlAsync(Input("Invalid check", height, weight)));
    File.WriteAllText(unsupportedFile, "Unsupported file type.");
    await Reject(() => api.CreateCatGirlAsync(Input("File type check", 150, 50, unsupportedFile)));
    using (var malformed = new MultipartFormDataContent())
    {
        malformed.Add(new StringContent("{invalid JSON"), "details");
        using var response = await http.PostAsync("api/catgirls/with-image", malformed);
        Check(response.StatusCode == System.Net.HttpStatusCode.BadRequest, "Malformed details should return 400.");
    }
    Check((await api.GetCatGirlsAsync()).Count == catalog.Count + 3, "Invalid input created advertisements.");

    await api.AddToCartAsync(login.UserId, created.Id);
    await api.AddToCartAsync(other.UserId, minimum.Id);
    await Reject(() => api.AddToCartAsync(login.UserId, created.Id));
    await Reject(() => api.PlaceOrderAsync(login.UserId, DateTime.Today.AddDays(-1), "Test"));
    Check((await api.GetCartAsync(login.UserId)).Count == 1, "Failed checkout cleared the cart.");
    var order = await api.PlaceOrderAsync(login.UserId, DateTime.Today.AddDays(1), "Budapest");
    orders.Add(order.Id);
    Check(order.Lines.Count == 1 && order.Lines[0].CatGirlId == created.Id && order.Lines[0].Name == created.Name, "Order lines were not saved.");
    Check((await api.GetCartAsync(login.UserId)).Count == 0, "Successful checkout did not clear submitted items.");
    Check((await api.GetCartAsync(other.UserId)).Count == 1, "Checkout touched another user's cart.");
    Check((await newSession.GetOrdersAsync(login.UserId)).Any(saved => saved.Id == order.Id), "Order history was not persisted.");
    Check((await newSession.GetOrdersAsync(other.UserId)).Count == 0, "Order history included another user's orders.");
    using (var update = await http.PutAsJsonAsync($"api/catgirls/{created.Id}", new
    {
        Name = "Renamed check", Age = 22, HeightCm = 300, WeightKg = 500,
        Phone = "+36 30 123 4567", Email = "check@example.com", Description = "Edited"
    })) update.EnsureSuccessStatusCode();
    Check((await newSession.GetCatGirlsAsync()).Single(cat => cat.Id == created.Id).ImagePath == created.ImagePath, "Editing details removed the image.");
    Check((await newSession.GetOrdersAsync(login.UserId)).Single(saved => saved.Id == order.Id).Lines[0].Name == created.Name, "Editing an advertisement changed a past order.");
    Console.WriteLine("PASS: API catalog/images, login, advertisement/image persistence, optional image, numeric boundaries/decimals, file-type check, cart isolation, failed/successful checkout and order history.");
}
catch (Exception error)
{
    Console.Error.WriteLine(error);
    Environment.ExitCode = 1;
}
finally
{
    foreach (var id in orders) (await http.DeleteAsync($"api/orders/{id}")).EnsureSuccessStatusCode();
    foreach (var id in cats) (await http.DeleteAsync($"api/catgirls/{id}")).EnsureSuccessStatusCode();
    foreach (var id in users) (await http.DeleteAsync($"api/users/{id}")).EnsureSuccessStatusCode();
    if (File.Exists(temporaryImage)) File.Delete(temporaryImage);
    if (File.Exists(unsupportedFile)) File.Delete(unsupportedFile);
}

static CatGirlInput Input(string name, double height, double weight, string? image = null) =>
    new(name, 22, height, weight, "+36 30 123 4567", "check@example.com", "API check", image);

static async Task Reject(Func<Task> action)
{
    try { await action(); }
    catch (InvalidOperationException) { return; }
    throw new Exception("Expected the operation to be rejected.");
}

static void Check(bool condition, string message)
{
    if (!condition) throw new Exception(message);
}
