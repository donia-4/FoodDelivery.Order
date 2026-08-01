using Order.Domain.Results;

namespace Order.Domain.Carts;

public static class CartErrors
{
    public static readonly Error InvalidCustomer = Error.Validation("Cart.Customer.Required", "Customer is required.");
    public static readonly Error InvalidRestaurant = Error.Validation("Cart.Restaurant.Required", "Restaurant is required.");
    public static readonly Error HasItems = Error.Conflict("Cart.HasItems", "Cannot change restaurant while cart has items.");
    public static readonly Error ItemFromDifferentRestaurant = Error.Conflict("Cart.Item.RestaurantMismatch", "Item must be from the same restaurant.");
    public static readonly Error ItemNotFound = Error.NotFound("Cart.Item.NotFound", "Item not found in cart.");
    public static readonly Error EmptyCart = Error.Validation("Cart.Empty", "Cannot checkout an empty cart.");
    public static readonly Error NotFound = Error.NotFound("Cart.NotFound", "Cart was not found.");
}