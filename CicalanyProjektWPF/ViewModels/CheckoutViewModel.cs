using System.Globalization;
using System.Windows.Input;
using Cicalany.Wpf.Commands;

namespace Cicalany.Wpf.ViewModels;

public sealed class CheckoutViewModel : ViewModelBase
{
    private static readonly string[] DateFormats =
    {
        "yyyy. MM. dd.", "yyyy. M. d.", "yyyy.MM.dd.", "yyyy.M.d.", "yyyy. MM. dd", "yyyy.MM.dd",
        "yyyy-MM-dd", "yyyy-M-d", "yyyy/MM/dd"
    };

    private readonly ShellViewModel _shell;
    private string _dateText = Format(DateTime.Today.AddDays(1));
    private string _location = "Online";

    public string DateText { get => _dateText; set => Set(ref _dateText, value); }
    public string Location { get => _location; set => Set(ref _location, value); }

    public ICommand PreviousDayCommand { get; }
    public ICommand NextDayCommand { get; }
    public ICommand TodayCommand { get; }
    public ICommand TomorrowCommand { get; }
    public ICommand NextWeekCommand { get; }
    public ICommand ConfirmCommand { get; }
    public ICommand BackCommand { get; }

    public CheckoutViewModel(ShellViewModel shell)
    {
        _shell = shell;
        PreviousDayCommand = new RelayCommand(() => Shift(-1));
        NextDayCommand = new RelayCommand(() => Shift(1));
        TodayCommand = new RelayCommand(() => SetFromToday(0));
        TomorrowCommand = new RelayCommand(() => SetFromToday(1));
        NextWeekCommand = new RelayCommand(() => SetFromToday(7));
        ConfirmCommand = new AsyncCommand(ConfirmAsync, _shell.ShowError);
        BackCommand = new RelayCommand(_shell.GoCart);
    }

    private static string Format(DateTime date) => date.ToString("yyyy. MM. dd.", CultureInfo.InvariantCulture);

    private static bool TryParseDate(string text, out DateTime date) =>
        DateTime.TryParseExact(text?.Trim(), DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out date);

    private void SetFromToday(int days) => DateText = Format(DateTime.Today.AddDays(days));

    // nyilak: a megadott dátumot egy nappal előre/hátra, de a mainál korábbra nem megy
    private void Shift(int days)
    {
        var current = TryParseDate(DateText, out var parsed) ? parsed : DateTime.Today;
        var next = current.AddDays(days);
        DateText = Format(next < DateTime.Today ? DateTime.Today : next);
    }

    private async Task ConfirmAsync()
    {
        if (!TryParseDate(DateText, out var date))
            throw new InvalidOperationException($"A dátum formátuma hibás. Például: {Format(DateTime.Today.AddDays(1))}");
        var order = await _shell.Api.PlaceOrderAsync(_shell.UserId, date, Location);
        _shell.GoCatalog();
        _shell.ShowMessage($"Rendelés #{order.Id} sikeresen leadva ({order.Lines.Count} cicálány).");
    }
}
