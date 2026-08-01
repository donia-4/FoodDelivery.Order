using Order.Domain.Common;
using Order.Domain.Orders;
using Order.Domain.Results;

namespace Order.Domain.Carts;

public sealed class Cart : AuditableEntity
{
    private readonly List<CartItem> _items = [];

    public Guid CustomerId { get; private set; }
    public Guid RestaurantId { get; private set; }

    public IReadOnlyCollection<CartItem> Items => _items;

    private Cart() { }

    private Cart(Guid id, Guid customerId, Guid restaurantId) : base(id)
    {
        CustomerId = customerId;
        RestaurantId = restaurantId;
    }

    public static Result<Cart> Create(Guid id, Guid customerId, Guid restaurantId)
    {
        if (customerId == Guid.Empty) return CartErrors.InvalidCustomer;
        if (restaurantId == Guid.Empty) return CartErrors.InvalidRestaurant;

        return new Cart(id, customerId, restaurantId);
    }

    public Result<Updated> ChangeRestaurant(Guid restaurantId)
    {
        if (restaurantId == Guid.Empty) return CartErrors.InvalidRestaurant;
        if (_items.Count > 0) return CartErrors.HasItems;

        RestaurantId = restaurantId;
        return Result.Updated;
    }

    public Result<Updated> AddItem(CartItem item)
    {
        if (item.RestaurantId != RestaurantId)
            return CartErrors.ItemFromDifferentRestaurant;

        var existing = _items.FirstOrDefault(i => i.MenuItemId == item.MenuItemId);
        if (existing is not null)
        {
            var updateResult = existing.UpdateQuantity(existing.Quantity + item.Quantity);
            if (updateResult.IsError) return updateResult.Errors;
            return Result.Updated;
        }

        _items.Add(item);
        return Result.Updated;
    }

    public Result<Updated> RemoveItem(Guid cartItemId)
    {
        var item = _items.FirstOrDefault(i => i.Id == cartItemId);
        if (item is null) return CartErrors.ItemNotFound;

        _items.Remove(item);
        return Result.Updated;
    }

    public Result<Updated> UpdateItemQuantity(Guid cartItemId, int quantity)
    {
        var item = _items.FirstOrDefault(i => i.Id == cartItemId);
        if (item is null) return CartErrors.ItemNotFound;

        var result = item.UpdateQuantity(quantity);
        if (result.IsError) return result.Errors;

        return Result.Updated;
    }

    public Result<Updated> Clear()
    {
        _items.Clear();
        return Result.Updated;
    }

    public Result<Orders.Order> Checkout(Guid orderId, string orderNumber, Guid addressId,
        decimal deliveryFee, decimal tax, decimal discount, string? notes = null)
    {
        if (_items.Count == 0) return CartErrors.EmptyCart;

        var subTotal = _items.Sum(i => i.Total);

        var orderResult = Orders.Order.Create(
            orderId, orderNumber, CustomerId, RestaurantId, addressId,
            subTotal, deliveryFee, tax, discount, notes);

        if (orderResult.IsError) return orderResult.Errors;

        var order = orderResult.Value;

        foreach (var item in _items)
        {
            var orderItemResult = OrderItem.Create(
                Guid.NewGuid(), order.Id, item.MenuItemId, item.Quantity, item.UnitPrice);

            if (orderItemResult.IsError) return orderItemResult.Errors;

            order.AddItem(orderItemResult.Value);
        }

        return order;
    }
}