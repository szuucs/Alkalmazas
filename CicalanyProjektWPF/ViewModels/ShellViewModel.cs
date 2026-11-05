using System.Windows.Input;
using Cicalany.Wpf.Commands;
using Cicalany.Wpf.Models;
using Cicalany.Wpf.Services;

namespace Cicalany.Wpf.ViewModels;

/// <summary>A főablak nézetmodellje: ez tartja a bejelentkezési állapotot és azt, hogy melyik oldal látszik.</summary>
public sealed class ShellViewModel : ViewModelBase
{
    // API nélküli mód. Az API bekötésekor ezt az egy sort kell cserélni.
    public IDataService Api { get; } = new LocalDataService();

    private object _currentPage = null!;
    public object CurrentPage { get => _currentPage; private set => Set(ref _currentPage, value); }

    private bool _isAuthenticated;
    public bool IsAuthenticated { get => _isAuthenticated; private set => Set(ref _isAuthenticated, value); }

    private string _userName = "";
    public string UserName { get => _userName; private set => Set(ref _userName, value); }

    private string _status = "Jelentkezz be vagy regisztrálj a kezdéshez. (Teszt belépés: demo / demo123)";
    public string Status { get => _status; private set => Set(ref _status, value); }

    public ICommand CatalogCommand { get; }
    public ICommand LogoutCommand { get; }

    public ShellViewModel()
    {
        CatalogCommand = new RelayCommand(GoCatalog);
        LogoutCommand = new RelayCommand(Logout);
        CurrentPage = new AuthViewModel(this);
    }

    public void SignedIn(LoginResult login)
    {
        Api.SetToken(login.Token);
        UserName = login.UserName;
        IsAuthenticated = true;
        ShowMessage($"Sikeres bejelentkezés: {login.UserName}");
        GoCatalog();
    }

    /// <summary>Egyelőre helyőrző oldal; ide kerül majd a katalógus.</summary>
    public void GoCatalog()
    {
        if (!IsAuthenticated) return;
        CurrentPage = new PlaceholderViewModel("Katalógus",
            "Ez az oldal még nincs kész. A bejelentkezés és a főablak már működik.");
    }

    public void ShowMessage(string message) => Status = message;
    public void ShowError(Exception error) => Status = "Hiba: " + error.Message;

    private void Logout()
    {
        Api.SetToken(null);
        IsAuthenticated = false;
        UserName = "";
        CurrentPage = new AuthViewModel(this);
        ShowMessage("Kijelentkeztél.");
    }
}
