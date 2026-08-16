namespace Order.Application.Common.Interfaces.Services
{
    public interface IRestaurantService
    {
        Task<string?> GetRestaurantNameAsync(Guid restaurantId, CancellationToken ct = default);

        /// <summary>
        /// Server-side source of truth for a menu item's current price and availability.
        /// Used so order/cart creation never has to trust a client-supplied price.
        /// NOTE: assumes a GET /api/menu-items/{id} endpoint on the restaurant service -
        /// adjust the route in RestaurantService.GetMenuItemAsync if the real contract differs.
        /// </summary>
        Task<MenuItemInfo?> GetMenuItemAsync(Guid menuItemId, CancellationToken ct = default);
    }

    public sealed record MenuItemInfo(
        Guid Id,
        Guid RestaurantId,
        string Name,
        decimal Price,
        bool IsAvailable);
}
