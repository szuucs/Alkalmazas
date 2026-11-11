using System.Globalization;

namespace Cicalany.Wpf.Models;

public sealed class LoginResult
{
    public string UserName { get; set; } = "";
    public int UserId { get; set; }
}

public sealed class CatGirlDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? ImagePath { get; set; }
    public int Age { get; set; }
    public double HeightCm { get; set; }
    public double WeightKg { get; set; }
    public string Description { get; set; } = "";
    // Alapból minden cicálány ára 20 000 Ft, és 2 órát vállal (az API küldi).
    public int Price { get; set; } = 20000;
    public int DurationHours { get; set; } = 2;
    // Elérhetőség: tárolódik, de a katalógus kártyáján nem jelenik meg.
    public string Phone { get; set; } = "";
    public string Email { get; set; } = "";
    public bool HasDescription => !string.IsNullOrWhiteSpace(Description);
    public string Summary => $"{Age} év  •  {HeightCm:0.#} cm  •  {WeightKg:0.#} kg";
    public string PriceLabel => $"{Price:N0} Ft";
    public string DurationLabel => $"{DurationHours} óra";
}

public sealed class CartItemDto
{
    public int Id { get; set; }
    public int CatGirlId { get; set; }
    public string Name { get; set; } = "";
    public int Age { get; set; }
    public double HeightCm { get; set; }
    public double WeightKg { get; set; }
    public int Price { get; set; }
    public int DurationHours { get; set; }
    public string AgeLabel => $"{Age} év";
    public string HeightLabel => $"{HeightCm:0.#} cm";
    public string WeightLabel => $"{WeightKg:0.#} kg";
    public string PriceLabel => $"{Price:N0} Ft";
    public string DurationLabel => $"{DurationHours} óra";
}

public sealed class OrderLineDto
{
    public int CatGirlId { get; set; }
    public string Name { get; set; } = "";
    public int Age { get; set; }
    public double HeightCm { get; set; }
    public double WeightKg { get; set; }
    public int Price { get; set; } = 20000;
    public int DurationHours { get; set; } = 2;
    public string AgeLabel => $"{Age} év";
    public string HeightLabel => $"{HeightCm:0.#} cm";
    public string WeightLabel => $"{WeightKg:0.#} kg";
    public string PriceLabel => $"{Price:N0} Ft";
}

public sealed class OrderDto
{
    public int Id { get; set; }
    // A foglalás kezdő időpontja (dátum és óra).
    public DateTime OrderDate { get; set; }
    public string Location { get; set; } = "";
    public int TotalPrice { get; set; }
    public int DurationHours { get; set; } = 2;
    public List<OrderLineDto> Lines { get; set; } = new();
    public string DateLabel => OrderDate.ToString("yyyy. MM. dd.");
    public string TimeLabel => TimeSlot.Range(OrderDate, DurationHours);
    public string TotalLabel => $"{TotalPrice:N0} Ft";
}

/// <summary>Új cicálány adatai (a leírás nem kötelező, a kép kötelező).</summary>
public sealed record CatGirlInput(string Name, int Age, double HeightCm, double WeightKg,
    string Phone, string Email, string? Description, string? ImagePath = null);

public static class TimeSlot
{
    /// <summary>
    /// A foglalás idősávja: egész órás kezdésnél "18-20", egyébként "13:31-15:31".
    /// Ha a vége átlóg éjfél után, a végére kerül, hogy "(másnap)".
    /// </summary>
    public static string Range(DateTime start, int hours)
    {
        var end = start.AddHours(hours);
        var text = start.Minute == 0
            ? $"{start.Hour}-{end.Hour}"
            : $"{start.ToString("HH:mm", CultureInfo.InvariantCulture)}-{end.ToString("HH:mm", CultureInfo.InvariantCulture)}";
        return end.Date > start.Date ? text + " (másnap)" : text;
    }
}
