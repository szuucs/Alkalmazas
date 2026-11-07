using System.Collections.ObjectModel;
using System.Windows.Input;
using Cicalany.Wpf.Commands;
using Cicalany.Wpf.Models;

namespace Cicalany.Wpf.ViewModels;

public sealed class CartViewModel : ViewModelBase
{
    private readonly ShellViewModel _shell;
    public ObservableCollection<CartItemDto> Items { get; } = new();

    private CartItemDto? _selectedItem;
    public CartItemDto? SelectedItem { get => _selectedItem; set => Set(ref _selectedItem, value); }

    public bool HasItems => Items.Count > 0;
    public string CountLabel => HasItems ? $"{Items.Count} cicálány a kosárban" : "A kosarad üres.";

    public ICommand RemoveCommand { get; }
    public ICommand CheckoutCommand { get; }

    public CartViewModel(ShellViewModel shell)
    {
        _shell = shell;
        RemoveCommand = new AsyncCommand(RemoveAsync, _shell.ShowError);
        CheckoutCommand = new RelayCommand(() => { if (HasItems) _shell.GoCheckout(); });
    }

    public async Task LoadAsync()
    {
        try
        {
            var items = await _shell.Api.GetCartAsync(_shell.UserId);
            Items.Clear();
            foreach (var item in items) Items.Add(item);
            SelectedItem = Items.FirstOrDefault();
            Notify(nameof(HasItems));
            Notify(nameof(CountLabel));
        }
        catch (Exception error) { _shell.ShowError(error); }
    }

    private async Task RemoveAsync()
    {
        if (SelectedItem is null) return;
        await _shell.Api.RemoveFromCartAsync(_shell.UserId, SelectedItem.Id);
        _shell.ShowMessage("Tétel eltávolítva.");
        await LoadAsync();
    }
}
