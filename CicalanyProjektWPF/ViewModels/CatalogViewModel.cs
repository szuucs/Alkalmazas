using System.Collections.ObjectModel;
using System.Windows.Input;
using Cicalany.Wpf.Commands;
using Cicalany.Wpf.Models;

namespace Cicalany.Wpf.ViewModels;

/// <summary>Egyszerű katalógus: itt lehet a cicálányokat a kosárba tenni.</summary>
public sealed class CatalogViewModel : ViewModelBase
{
    private readonly ShellViewModel _shell;
    public ObservableCollection<CatGirlDto> CatGirls { get; } = new();
    public bool IsEmpty => CatGirls.Count == 0;
    public ICommand AddToCartCommand { get; }
    public ICommand NewCommand { get; }

    public CatalogViewModel(ShellViewModel shell)
    {
        _shell = shell;
        AddToCartCommand = new AsyncCommand<CatGirlDto>(AddToCartAsync, _shell.ShowError);
        NewCommand = new RelayCommand(_shell.GoAddCatGirl);
    }

    public async Task LoadAsync()
    {
        try
        {
            var list = await _shell.Api.GetCatGirlsAsync();
            CatGirls.Clear();
            foreach (var cat in list) CatGirls.Add(cat);
            Notify(nameof(IsEmpty));
        }
        catch (Exception error) { _shell.ShowError(error); }
    }

    private async Task AddToCartAsync(CatGirlDto cat)
    {
        await _shell.Api.AddToCartAsync(_shell.UserId, cat.Id);
        _shell.ShowMessage($"{cat.Name} bekerült a kosárba.");
    }
}
