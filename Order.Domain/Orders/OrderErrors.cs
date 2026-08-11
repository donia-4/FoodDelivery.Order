using Order.Domain.Results;

namespace Order.Domain.Orders;

public static class OrderErrors
{
    public static readonly Error InvalidOrderNumber = Error.Validation("Order.OrderNumber.Required", "Order number is required.");
    public static readonly Error InvalidCustomer = Error.Validation("Order.Customer.Required", "Customer is required.");
    public static readonly Error InvalidRestaurant = Error.Validation("Order.Restaurant.Required", "Restaurant is required.");
    public static readonly Error InvalidAddress = Error.Validation("Order.Address.Required", "Delivery address is required.");
    public static readonly Error InvalidSubTotal = Error.Validation("Order.SubTotal.Invalid", "Subtotal must be zero or greater.");
    public static readonly Error InvalidDeliveryFee = Error.Validation("Order.DeliveryFee.Invalid", "Delivery fee must be zero or greater.");
    public static readonly Error InvalidTax = Error.Validation("Order.Tax.Invalid", "Tax must be zero or greater.");
    public static readonly Error InvalidDiscount = Error.Validation("Order.Discount.Invalid", "Discount must be zero or greater.");
    public static readonly Error InvalidStatusTransition = Error.Conflict("Order.Status.InvalidTransition", "Invalid status transition.");
    public static readonly Error CannotModifyAfterConfirmation = Error.Conflict("Order.Modify.AfterConfirmation", "Cannot modify order after confirmation.");
    public static readonly Error CannotCancel = Error.Conflict("Order.Cancel.NotAllowed", "Order cannot be cancelled at this stage.");
    public static readonly Error NotFound = Error.NotFound("Order.NotFound", "Order was not found.");
    public static readonly Error RestaurantNotFound = Error.NotFound("Order.Restaurant.NotFound", "Restaurant was not found or is not available.");
    public static readonly Error MenuItemNotFound = Error.NotFound("Order.MenuItem.NotFound", "One or more menu items were not found.");
    public static readonly Error MenuItemUnavailable = Error.Conflict("Order.MenuItem.Unavailable", "One or more menu items are currently unavailable.");
    public static readonly Error MenuItemFromDifferentRestaurant = Error.Validation("Order.MenuItem.RestaurantMismatch", "One or more menu items do not belong to the selected restaurant.");
    public static readonly Error EmptyItems = Error.Validation("Order.Items.Empty", "Order must contain at least one item.");
    public static readonly Error ItemNotFound = Error.NotFound("Order.Item.NotFound", "Item not found in this order.");
}