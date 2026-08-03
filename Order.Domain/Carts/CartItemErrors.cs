using Order.Domain.Results;

namespace Order.Domain.Carts;

public static class CartItemErrors
{
    public static readonly Error InvalidCart = Error.Validation("CartItem.Cart.Required", "Cart is required.");
    public static readonly Error InvalidMenuItem = Error.Validation("CartItem.MenuItem.Required", "Menu item is required.");
    public static readonly Error InvalidRestaurant = Error.Validation("CartItem.Restaurant.Required", "Restaurant is required.");
    public static readonly Error InvalidQuantity = Error.Validation("CartItem.Quantity.Invalid", "Quantity must be greater than zero.");
    public static readonly Error InvalidUnitPrice = Error.Validation("CartItem.UnitPrice.Invalid", "Unit price must be zero or greater.");
}