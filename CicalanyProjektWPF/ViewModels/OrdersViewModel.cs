using System.Collections.ObjectModel;
using System.Windows.Input;
using Cicalany.Wpf.Commands;
using Cicalany.Wpf.Models;

namespace Cicalany.Wpf.ViewModels;

/// <summary>Csak olvasható lista a leadott rendelésekről.</summary>
public sealed class OrdersViewModel : ViewModelBase
{
    private readonly ShellViewModel _shell;
    public ObservableCollection<OrderDto> Orders { get; } = new();
    public bool IsEmpty => Orders.Count == 0;
    public ICommand RefreshCommand { get; }

    public OrdersViewModel(ShellViewModel shell)
    {
        _shell = shell;
        RefreshCommand = new AsyncCommand(LoadAsync, _shell.ShowError);
    }

    public async Task LoadAsync()
    {
        try
        {
            var orders = await _shell.Api.GetOrdersAsync(_shell.UserId);
            Orders.Clear();
            foreach (var order in orders) Orders.Add(order);
            Notify(nameof(IsEmpty));
        }
        catch (Exception error) { _shell.ShowError(error); }
    }
}
