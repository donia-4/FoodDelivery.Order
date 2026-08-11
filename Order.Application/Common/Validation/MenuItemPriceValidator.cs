using Order.Application.Common.Interfaces.Services;
using Order.Domain.Orders;
using Order.Domain.Results;

namespace Order.Application.Common.Validation;

public sealed record ValidatedMenuItem(Guid MenuItemId, int Quantity, decimal UnitPrice);

public static class MenuItemPriceValidator
{
    public static async Task<Result<List<ValidatedMenuItem>>> ValidateAsync(
        IRestaurantService restaurantService,
        Guid restaurantId,
        IEnumerable<(Guid MenuItemId, int Quantity)> items,
        CancellationToken ct)
    {
        var validated = new List<ValidatedMenuItem>();

        foreach (var (menuItemId, quantity) in items)
        {
            var menuItem = await restaurantService.GetMenuItemAsync(menuItemId, ct);

            if (menuItem is null)
                return OrderErrors.MenuItemNotFound;

            if (menuItem.RestaurantId != restaurantId)
                return OrderErrors.MenuItemFromDifferentRestaurant;

            if (!menuItem.IsAvailable)
                return OrderErrors.MenuItemUnavailable;

            validated.Add(new ValidatedMenuItem(menuItemId, quantity, menuItem.Price));
        }

        return validated;
    }
}
