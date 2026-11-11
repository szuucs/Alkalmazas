using System.Globalization;
using System.Windows.Input;
using Cicalany.Wpf.Commands;
using Cicalany.Wpf.Models;

namespace Cicalany.Wpf.ViewModels;

public sealed class CheckoutViewModel : ViewModelBase
{
    private static readonly string[] DateFormats =
    {
        "yyyy. MM. dd.", "yyyy. M. d.", "yyyy.MM.dd.", "yyyy.M.d.", "yyyy. MM. dd", "yyyy.MM.dd",
        "yyyy-MM-dd", "yyyy-M-d", "yyyy/MM/dd"
    };

    // 18:00, 8:30, 18.30, vagy csak az óra (18); az egyjegyű "%H" a .NET egyedi formátumjelölése
    private static readonly string[] TimeFormats = { "H:mm", "HH:mm", "H.mm", "HH.mm", "%H", "HH" };

    private readonly ShellViewModel _shell;
    private string _dateText = FormatDate(DateTime.Today.AddDays(1));
    private string _timeText = "18:00";
    private string _location = "Online";
    private int _itemCount;
    private int _total;
    private int _durationHours = 2;

    public string DateText { get => _dateText; set => Set(ref _dateText, value); }
    public string Location { get => _location; set => Set(ref _location, value); }

    public string TimeText
    {
        get => _timeText;
        set { if (Set(ref _timeText, value)) Notify(nameof(AvailabilityText)); }
    }

    /// <summary>Pl. "Elérhető: 18-20" vagy "Elérhető: 13:31-15:31" (a kezdéstől számított vállalt időtartammal).</summary>
    public string AvailabilityText =>
        TryParseTime(TimeText, out var time)
            ? $"Elérhető: {TimeSlot.Range(DateTime.Today + time, _durationHours)}"
            : "Adj meg érvényes időpontot, például 18:00.";

    public string DurationText => $"Minden cicálány {_durationHours} órát vállal.";
    public string SummaryText => _itemCount == 0 ? "" : $"{_itemCount} cicálány  •  összesen {_total:N0} Ft";

    public ICommand PreviousDayCommand { get; }
    public ICommand NextDayCommand { get; }
    public ICommand TodayCommand { get; }
    public ICommand TomorrowCommand { get; }
    public ICommand NextWeekCommand { get; }
    public ICommand PreviousHourCommand { get; }
    public ICommand NextHourCommand { get; }
    public ICommand ConfirmCommand { get; }
    public ICommand BackCommand { get; }

    public CheckoutViewModel(ShellViewModel shell)
    {
        _shell = shell;
        PreviousDayCommand = new RelayCommand(() => ShiftDay(-1));
        NextDayCommand = new RelayCommand(() => ShiftDay(1));
        TodayCommand = new RelayCommand(() => SetDateFromToday(0));
        TomorrowCommand = new RelayCommand(() => SetDateFromToday(1));
        NextWeekCommand = new RelayCommand(() => SetDateFromToday(7));
        PreviousHourCommand = new RelayCommand(() => ShiftHour(-1));
        NextHourCommand = new RelayCommand(() => ShiftHour(1));
        ConfirmCommand = new AsyncCommand(ConfirmAsync, _shell.ShowError);
        BackCommand = new RelayCommand(_shell.GoCart);
    }

    /// <summary>A kosár tartalmából az összeg és az időtartam; a rendelés előtt ezt látja a felhasználó.</summary>
    public async Task LoadAsync()
    {
        try
        {
            var items = await _shell.Api.GetCartAsync(_shell.UserId);
            _itemCount = items.Count;
            _total = items.Sum(item => item.Price);
            _durationHours = items.Count == 0 ? 2 : items.Max(item => item.DurationHours);
            Notify(nameof(SummaryText));
            Notify(nameof(DurationText));
            Notify(nameof(AvailabilityText));
        }
        catch (Exception error) { _shell.ShowError(error); }
    }

    private static string FormatDate(DateTime date) => date.ToString("yyyy. MM. dd.", CultureInfo.InvariantCulture);

    private static bool TryParseDate(string? text, out DateTime date) =>
        DateTime.TryParseExact(text?.Trim(), DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out date);

    private static bool TryParseTime(string? text, out TimeSpan time)
    {
        if (DateTime.TryParseExact(text?.Trim(), TimeFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
        {
            time = parsed.TimeOfDay;
            return true;
        }
        time = default;
        return false;
    }

    private void SetDateFromToday(int days) => DateText = FormatDate(DateTime.Today.AddDays(days));

    // nyilak: a megadott dátumot egy nappal előre/hátra, de a mainál korábbra nem megy
    private void ShiftDay(int days)
    {
        var current = TryParseDate(DateText, out var parsed) ? parsed : DateTime.Today;
        var next = current.AddDays(days);
        DateText = FormatDate(next < DateTime.Today ? DateTime.Today : next);
    }

    // nyilak: az időpont egy órával előre/hátra (a percek maradnak), a nap körbeér
    private void ShiftHour(int hours)
    {
        var current = TryParseTime(TimeText, out var parsed) ? parsed : new TimeSpan(18, 0, 0);
        var minutes = ((int)current.TotalMinutes + hours * 60) % 1440;
        if (minutes < 0) minutes += 1440;
        TimeText = new TimeSpan(0, minutes, 0).ToString(@"hh\:mm");
    }

    private async Task ConfirmAsync()
    {
        if (!TryParseDate(DateText, out var date))
            throw new InvalidOperationException($"A dátum formátuma hibás. Például: {FormatDate(DateTime.Today.AddDays(1))}");
        if (!TryParseTime(TimeText, out var time))
            throw new InvalidOperationException("Az időpont formátuma hibás. Például: 18:00");

        var order = await _shell.Api.PlaceOrderAsync(_shell.UserId, date.Date + time, Location);
        _shell.GoCatalog();
        _shell.ShowMessage($"Rendelés #{order.Id} leadva: {order.DateLabel}, {order.TimeLabel} ({order.Lines.Count} cicálány). Fizetés a helyszínen.");
    }
}
