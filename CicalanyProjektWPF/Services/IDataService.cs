using Cicalany.Wpf.Models;

namespace Cicalany.Wpf.Services;

/// <summary>
/// A WPF az adatait ezen a felületen keresztül éri el. A LocalDataService a hitelesítést
/// az API-n végzi, a katalógust, kosarat és rendeléseket egyelőre memóriában tárolja.
/// Token nincs: ahol kell, a hívó adja át a bejelentkezett felhasználó azonosítóját.
/// </summary>
public interface IDataService
{
    Task<LoginResult> RegisterAsync(string userName, string password);
    Task<LoginResult> LoginAsync(string userName, string password);

    Task<List<CatGirlDto>> GetCatGirlsAsync();
    Task<CatGirlDto> CreateCatGirlAsync(CatGirlInput input);

    Task<List<CartItemDto>> GetCartAsync(int userId);
    Task<CartItemDto> AddToCartAsync(int userId, int catGirlId);
    Task RemoveFromCartAsync(int userId, int cartItemId);

    Task<OrderDto> PlaceOrderAsync(int userId, DateTime date, string location);
    Task<List<OrderDto>> GetOrdersAsync(int userId);
}
