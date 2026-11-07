using System.Windows.Input;
using Cicalany.Wpf.Commands;

namespace Cicalany.Wpf.ViewModels;

public sealed class CheckoutViewModel : ViewModelBase
{
    private readonly ShellViewModel _shell;
    private DateTime? _date = DateTime.Today.AddDays(1);
    private string _location = "Online";

    public DateTime? Date { get => _date; set => Set(ref _date, value); }
    public string Location { get => _location; set => Set(ref _location, value); }

    public ICommand ConfirmCommand { get; }
    public ICommand BackCommand { get; }

    public CheckoutViewModel(ShellViewModel shell)
    {
        _shell = shell;
        ConfirmCommand = new AsyncCommand(ConfirmAsync, _shell.ShowError);
        BackCommand = new RelayCommand(_shell.GoCart);
    }

    private async Task ConfirmAsync()
    {
        if (Date is null) throw new InvalidOperationException("Válassz dátumot.");
        var order = await _shell.Api.PlaceOrderAsync(_shell.UserId, Date.Value, Location);
        _shell.GoCatalog();
        _shell.ShowMessage($"Rendelés #{order.Id} sikeresen leadva ({order.Lines.Count} cicálány).");
    }
}
