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
    // Elérhetőség: tárolódik, de a katalógus kártyáján nem jelenik meg.
    public string Phone { get; set; } = "";
    public string Email { get; set; } = "";
    public bool HasDescription => !string.IsNullOrWhiteSpace(Description);
    public string Summary => $"{Age} év  •  {HeightCm:0.#} cm  •  {WeightKg:0.#} kg";
}

public sealed class CartItemDto
{
    public int Id { get; set; }
    public int CatGirlId { get; set; }
    public string Name { get; set; } = "";
    public int Age { get; set; }
    public double HeightCm { get; set; }
    public double WeightKg { get; set; }
    public string AgeLabel => $"{Age} év";
    public string HeightLabel => $"{HeightCm:0.#} cm";
    public string WeightLabel => $"{WeightKg:0.#} kg";
}

public sealed class OrderLineDto
{
    public int CatGirlId { get; set; }
    public string Name { get; set; } = "";
    public int Age { get; set; }
    public double HeightCm { get; set; }
    public double WeightKg { get; set; }
    public string AgeLabel => $"{Age} év";
    public string HeightLabel => $"{HeightCm:0.#} cm";
    public string WeightLabel => $"{WeightKg:0.#} kg";
}

public sealed class OrderDto
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }
    public string Location { get; set; } = "";
    public List<OrderLineDto> Lines { get; set; } = new();
    public string DateLabel => OrderDate.ToString("yyyy. MM. dd.");
}

/// <summary>Új cicálány adatai (a leírás nem kötelező).</summary>
public sealed record CatGirlInput(string Name, int Age, double HeightCm, double WeightKg,
    string Phone, string Email, string? Description, string? ImagePath = null);
