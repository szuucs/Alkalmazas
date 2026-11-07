using Cicalany.Wpf.Models;

namespace Cicalany.Wpf.Services;

/// <summary>
/// A WPF az adatait ezen a felületen keresztül éri el. Most a LocalDataService valósítja meg
/// (memóriában, API nélkül), később egy API-t hívó osztályra cserélhető a ShellViewModel-ben.
/// Token nincs: ahol kell, a hívó adja át a bejelentkezett felhasználó azonosítóját.
/// </summary>
public interface IDataService
{
    Task<LoginResult> RegisterAsync(string userName, string password);
    Task<LoginResult> LoginAsync(string userName, string password);

    Task<List<CatGirlDto>> GetCatGirlsAsync();

    Task<List<CartItemDto>> GetCartAsync(int userId);
    Task<CartItemDto> AddToCartAsync(int userId, int catGirlId);
    Task RemoveFromCartAsync(int userId, int cartItemId);

    Task<OrderDto> PlaceOrderAsync(int userId, DateTime date, string location);
}
