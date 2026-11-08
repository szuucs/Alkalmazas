using System.Globalization;
using System.Windows.Input;
using Cicalany.Wpf.Commands;
using Cicalany.Wpf.Models;

namespace Cicalany.Wpf.ViewModels;

/// <summary>Új cicálány felvétele. A név, életkor, magasság és súly kötelező, a leírás nem.</summary>
public sealed class AddCatGirlViewModel : ViewModelBase
{
    private readonly ShellViewModel _shell;
    private string _name = "";
    private string _age = "";
    private string _height = "";
    private string _weight = "";
    private string _phone = "";
    private string _email = "";
    private string _description = "";

    public string Name { get => _name; set => Set(ref _name, value); }
    public string Age { get => _age; set => Set(ref _age, value); }
    public string Height { get => _height; set => Set(ref _height, value); }
    public string Weight { get => _weight; set => Set(ref _weight, value); }
    public string Phone { get => _phone; set => Set(ref _phone, value); }
    public string Email { get => _email; set => Set(ref _email, value); }
    public string Description { get => _description; set => Set(ref _description, value); }

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    public AddCatGirlViewModel(ShellViewModel shell)
    {
        _shell = shell;
        SaveCommand = new AsyncCommand(SaveAsync, _shell.ShowError);
        CancelCommand = new RelayCommand(_shell.GoCatalog);
    }

    private async Task SaveAsync()
    {
        var name = Name.Trim();
        if (name.Length == 0) throw new InvalidOperationException("A név megadása kötelező.");
        var age = ParseAge(Age);
        var height = ParseNumber(Height, "A magasság", "cm");
        var weight = ParseNumber(Weight, "A súly", "kg");
        if (string.IsNullOrWhiteSpace(Phone)) throw new InvalidOperationException("A telefonszám megadása kötelező.");
        if (string.IsNullOrWhiteSpace(Email)) throw new InvalidOperationException("Az e-mail cím megadása kötelező.");

        var created = await _shell.Api.CreateCatGirlAsync(new CatGirlInput(name, age, height, weight, Phone, Email, Description));
        _shell.GoCatalog();
        _shell.ShowMessage($"{created.Name} hozzáadva a katalógushoz.");
    }

    private static int ParseAge(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) throw new InvalidOperationException("Az életkor megadása kötelező.");
        if (!int.TryParse(text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var age))
            throw new InvalidOperationException("Az életkor egész szám legyen (években).");
        return age;
    }

    // tizedesvesszőt és tizedespontot is elfogad
    private static double ParseNumber(string text, string label, string unit)
    {
        if (string.IsNullOrWhiteSpace(text)) throw new InvalidOperationException($"{label} megadása kötelező.");
        var normalized = text.Trim().Replace(',', '.');
        if (!double.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            throw new InvalidOperationException($"{label} szám legyen ({unit}).");
        return value;
    }
}
