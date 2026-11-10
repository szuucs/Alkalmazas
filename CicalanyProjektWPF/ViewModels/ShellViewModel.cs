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

    /// <summary>A bejelentkezett felhasználó azonosítója (kijelentkezve 0).</summary>
    public int UserId { get; private set; }

    private string _userName = "";
    public string UserName { get => _userName; private set => Set(ref _userName, value); }

    private string _status = "Jelentkezz be vagy regisztrálj a kezdéshez. (Teszt belépés: demo / demo123)";
    public string Status { get => _status; private set => Set(ref _status, value); }

    public ICommand CatalogCommand { get; }
    public ICommand CartCommand { get; }
    public ICommand OrdersCommand { get; }
    public ICommand LogoutCommand { get; }

    public ShellViewModel()
    {
        CatalogCommand = new RelayCommand(GoCatalog);
        CartCommand = new RelayCommand(GoCart);
        OrdersCommand = new RelayCommand(GoOrders);
        LogoutCommand = new RelayCommand(Logout);
        CurrentPage = new AuthViewModel(this);
    }

    public void SignedIn(LoginResult login)
    {
        UserId = login.UserId;
        UserName = login.UserName;
        IsAuthenticated = true;
        ShowMessage($"Sikeres bejelentkezés: {login.UserName}");
        GoCatalog();
    }

    public void GoCatalog()
    {
        if (!IsAuthenticated) return;
        var page = new CatalogViewModel(this);
        CurrentPage = page;
        _ = page.LoadAsync();
    }

    public void GoAddCatGirl()
    {
        if (!IsAuthenticated) return;
        CurrentPage = new AddCatGirlViewModel(this);
    }

    public void GoCart()
    {
        if (!IsAuthenticated) return;
        var page = new CartViewModel(this);
        CurrentPage = page;
        _ = page.LoadAsync();
    }

    public void GoOrders()
    {
        if (!IsAuthenticated) return;
        var page = new OrdersViewModel(this);
        CurrentPage = page;
        _ = page.LoadAsync();
    }

    public void GoCheckout()
    {
        if (!IsAuthenticated) return;
        CurrentPage = new CheckoutViewModel(this);
    }

    public void ShowMessage(string message) => Status = message;
    public void ShowError(Exception error) => Status = "Hiba: " + error.Message;

    private void Logout()
    {
        IsAuthenticated = false;
        UserId = 0;
        UserName = "";
        CurrentPage = new AuthViewModel(this);
        ShowMessage("Kijelentkeztél.");
    }
}
