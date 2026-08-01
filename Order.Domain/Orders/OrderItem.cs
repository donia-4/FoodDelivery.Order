using Order.Domain.Common;
using Order.Domain.Results;


namespace Order.Domain.Orders;

public sealed class OrderItem : AuditableEntity
{
    public Guid OrderId { get; private set; }
    public Guid MenuItemId { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal Total { get; private set; }

    private OrderItem() { }

    private OrderItem(Guid id, Guid orderId, Guid menuItemId, int quantity, decimal unitPrice)
        : base(id)
    {
        OrderId = orderId;
        MenuItemId = menuItemId;
        Quantity = quantity;
        UnitPrice = unitPrice;
        Total = quantity * unitPrice;
    }

    public static Result<OrderItem> Create(Guid id, Guid orderId, Guid menuItemId, int quantity, decimal unitPrice)
    {
        if (orderId == Guid.Empty) return OrderItemErrors.InvalidOrder;
        if (menuItemId == Guid.Empty) return OrderItemErrors.InvalidMenuItem;
        if (quantity <= 0) return OrderItemErrors.InvalidQuantity;
        if (unitPrice < 0) return OrderItemErrors.InvalidUnitPrice;

        return new OrderItem(id, orderId, menuItemId, quantity, unitPrice);
    }

    public Result<Updated> UpdateQuantity(int quantity)
    {
        if (quantity <= 0) return OrderItemErrors.InvalidQuantity;

        Quantity = quantity;
        Total = Quantity * UnitPrice;
        return Result.Updated;
    }
}

