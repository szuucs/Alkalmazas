using Cicalany.Wpf.Models;

namespace Cicalany.Wpf.Services;

/// <summary>
/// A WPF az adatait ezen a felületen keresztül éri el. Most a LocalDataService valósítja meg
/// (memóriában, API nélkül), később egy API-t hívó osztályra cserélhető a ShellViewModel-ben.
/// Új funkciónál (katalógus, kosár, ...) ide kerülnek az új metódusok.
/// </summary>
public interface IDataService
{
    void SetToken(string? token);
    Task<LoginResult> RegisterAsync(string userName, string password);
    Task<LoginResult> LoginAsync(string userName, string password);
}
