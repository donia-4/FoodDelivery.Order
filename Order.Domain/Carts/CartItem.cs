using Order.Domain.Common;
using Order.Domain.Results;

namespace Order.Domain.Carts;

public sealed class CartItem : AuditableEntity
{
    public Guid CartId { get; private set; }
    public Guid MenuItemId { get; private set; }
    public Guid RestaurantId { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal Total => Quantity * UnitPrice;
    public string? Notes { get; private set; }

    private CartItem() { }

    private CartItem(Guid id, Guid cartId, Guid menuItemId, Guid restaurantId,
        int quantity, decimal unitPrice, string? notes) : base(id)
    {
        CartId = cartId;
        MenuItemId = menuItemId;
        RestaurantId = restaurantId;
        Quantity = quantity;
        UnitPrice = unitPrice;
        Notes = notes;
    }

    public static Result<CartItem> Create(Guid id, Guid cartId, Guid menuItemId,
        Guid restaurantId, int quantity, decimal unitPrice, string? notes = null)
    {
        if (cartId == Guid.Empty) return CartItemErrors.InvalidCart;
        if (menuItemId == Guid.Empty) return CartItemErrors.InvalidMenuItem;
        if (restaurantId == Guid.Empty) return CartItemErrors.InvalidRestaurant;
        if (quantity <= 0) return CartItemErrors.InvalidQuantity;
        if (unitPrice < 0) return CartItemErrors.InvalidUnitPrice;

        return new CartItem(id, cartId, menuItemId, restaurantId, quantity, unitPrice, notes?.Trim());
    }

    public Result<Updated> UpdateQuantity(int quantity)
    {
        if (quantity <= 0) return CartItemErrors.InvalidQuantity;
        Quantity = quantity;
        return Result.Updated;
    }
}