using System.Windows.Input;
using Cicalany.Wpf.Commands;

namespace Cicalany.Wpf.ViewModels;

public sealed class AuthViewModel : ViewModelBase
{
    private readonly ShellViewModel _shell;
    private string _userName = "";
    private string _password = "";
    public string UserName { get => _userName; set => Set(ref _userName, value); }
    public string Password { get => _password; set => Set(ref _password, value); }
    public ICommand LoginCommand { get; }
    public ICommand RegisterCommand { get; }

    public AuthViewModel(ShellViewModel shell)
    {
        _shell = shell;
        LoginCommand = new AsyncCommand(LoginAsync, _shell.ShowError);
        RegisterCommand = new AsyncCommand(RegisterAsync, _shell.ShowError);
    }

    private async Task LoginAsync() =>
        _shell.SignedIn(await _shell.Api.LoginAsync(UserName, Password));

    private async Task RegisterAsync() =>
        _shell.SignedIn(await _shell.Api.RegisterAsync(UserName, Password));
}
