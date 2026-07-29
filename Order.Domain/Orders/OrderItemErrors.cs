using Order.Domain.Results;

namespace Order.Domain.Orders;

public static class OrderItemErrors
{
    public static readonly Error InvalidOrder = Error.Validation("OrderItem.Order.Required", "Order is required.");
    public static readonly Error InvalidMenuItem = Error.Validation("OrderItem.MenuItem.Required", "Menu item is required.");
    public static readonly Error InvalidQuantity = Error.Validation("OrderItem.Quantity.Invalid", "Quantity must be greater than zero.");
    public static readonly Error InvalidUnitPrice = Error.Validation("OrderItem.UnitPrice.Invalid", "Unit price must be zero or greater.");
    public static readonly Error NotFound = Error.NotFound("OrderItem.NotFound", "Order item was not found.");
}