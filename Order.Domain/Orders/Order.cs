using Order.Domain.Common;
using Order.Domain.Orders;
using Order.Domain.Orders.Enums;
using Order.Domain.Results;


namespace Order.Domain.Orders;

public sealed class Order : AuditableEntity
{
    private readonly List<OrderItem> _items = [];
    private readonly List<OrderStatusHistory> _statusHistory = [];

    public string OrderNumber { get; private set; } = string.Empty;
    public Guid CustomerId { get; private set; }
    public Guid RestaurantId { get; private set; }
    public Guid AddressId { get; private set; }
    public OrderStatus Status { get; private set; }
    public decimal SubTotal { get; private set; }
    public decimal DeliveryFee { get; private set; }
    public decimal Tax { get; private set; }
    public decimal Discount { get; private set; }
    public decimal Total { get; private set; }
    public string? Notes { get; private set; }

    public IReadOnlyCollection<OrderItem> Items => _items;
    public IReadOnlyCollection<OrderStatusHistory> StatusHistory => _statusHistory;

    private Order() { }

    private Order(Guid id, string orderNumber, Guid customerId, Guid restaurantId, Guid addressId,
        decimal subTotal, decimal deliveryFee, decimal tax, decimal discount, string? notes)
        : base(id)
    {
        OrderNumber = orderNumber;
        CustomerId = customerId;
        RestaurantId = restaurantId;
        AddressId = addressId;
        Status = OrderStatus.Pending;
        SubTotal = subTotal;
        DeliveryFee = deliveryFee;
        Tax = tax;
        Discount = discount;
        Notes = notes;
        Total = CalculateTotal();
    }

    public static Result<Order> Create(Guid id, string orderNumber, Guid customerId, Guid restaurantId, Guid addressId,
        decimal subTotal, decimal deliveryFee, decimal tax, decimal discount, string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(orderNumber)) return OrderErrors.InvalidOrderNumber;
        if (customerId == Guid.Empty) return OrderErrors.InvalidCustomer;
        if (restaurantId == Guid.Empty) return OrderErrors.InvalidRestaurant;
        if (addressId == Guid.Empty) return OrderErrors.InvalidAddress;
        if (subTotal < 0) return OrderErrors.InvalidSubTotal;
        if (deliveryFee < 0) return OrderErrors.InvalidDeliveryFee;
        if (tax < 0) return OrderErrors.InvalidTax;
        if (discount < 0) return OrderErrors.InvalidDiscount;

        return new Order(id, orderNumber.Trim(), customerId, restaurantId, addressId, subTotal, deliveryFee, tax, discount, notes?.Trim());
    }

    private decimal CalculateTotal() => SubTotal + DeliveryFee + Tax - Discount;

    private Result<Updated> RecalculateTotal()
    {
        Total = CalculateTotal();
        return Result.Updated;
    }

    // ========== UPDATE ==========

    public Result<Updated> Update(
        Guid? addressId = null,
        string? notes = null,
        decimal? deliveryFee = null,
        decimal? tax = null,
        decimal? discount = null)
    {
        if (Status != OrderStatus.Pending)
            return OrderErrors.CannotModifyAfterConfirmation;

        if (addressId.HasValue)
        {
            if (addressId.Value == Guid.Empty) return OrderErrors.InvalidAddress;
            AddressId = addressId.Value;
        }

        if (notes is not null)
        {
            Notes = notes.Trim();
        }

        if (deliveryFee.HasValue)
        {
            if (deliveryFee.Value < 0) return OrderErrors.InvalidDeliveryFee;
            DeliveryFee = deliveryFee.Value;
        }

        if (tax.HasValue)
        {
            if (tax.Value < 0) return OrderErrors.InvalidTax;
            Tax = tax.Value;
        }

        if (discount.HasValue)
        {
            if (discount.Value < 0) return OrderErrors.InvalidDiscount;
            Discount = discount.Value;
        }

        return RecalculateTotal();
    }

    // ========== ITEMS MANAGEMENT ==========

    public Result<Updated> AddItem(OrderItem item)
    {
        if (Status != OrderStatus.Pending)
            return OrderErrors.CannotModifyAfterConfirmation;

        _items.Add(item);
        SubTotal += item.Total;
        return RecalculateTotal();
    }

    public Result<Updated> RemoveItem(OrderItem item)
    {
        if (Status != OrderStatus.Pending)
            return OrderErrors.CannotModifyAfterConfirmation;

        if (!_items.Remove(item))
            return OrderErrors.ItemNotFound;

        SubTotal -= item.Total;
        if (SubTotal < 0) SubTotal = 0;
        return RecalculateTotal();
    }

    public Result<Updated> UpdateItemQuantity(OrderItem item, int quantity)
    {
        if (Status != OrderStatus.Pending)
            return OrderErrors.CannotModifyAfterConfirmation;

        if (!_items.Contains(item))
            return OrderErrors.ItemNotFound;

        var oldTotal = item.Total;
        var updateResult = item.UpdateQuantity(quantity);
        if (updateResult.IsError) return updateResult.Errors;

        SubTotal = SubTotal - oldTotal + item.Total;
        return RecalculateTotal();
    }

    // ========== STATUS TRANSITIONS ==========

    public Result<Updated> Accept(string changedBy)
    {
        if (Status != OrderStatus.Pending)
            return OrderErrors.InvalidStatusTransition;

        return TransitionTo(OrderStatus.Accepted, changedBy);
    }

    public Result<Updated> Reject(string changedBy, string? reason = null)
    {
        if (Status != OrderStatus.Pending)
            return OrderErrors.InvalidStatusTransition;

        return TransitionTo(OrderStatus.Rejected, changedBy);
    }

    public Result<Updated> StartPreparing(string changedBy)
    {
        if (Status != OrderStatus.Accepted)
            return OrderErrors.InvalidStatusTransition;

        return TransitionTo(OrderStatus.Preparing, changedBy);
    }

    public Result<Updated> MarkReady(string changedBy)
    {
        if (Status != OrderStatus.Preparing)
            return OrderErrors.InvalidStatusTransition;

        return TransitionTo(OrderStatus.Ready, changedBy);
    }

    public Result<Updated> MarkOutForDelivery(string changedBy)
    {
        if (Status != OrderStatus.Ready)
            return OrderErrors.InvalidStatusTransition;

        return TransitionTo(OrderStatus.OutForDelivery, changedBy);
    }

    public Result<Updated> MarkDelivered(string changedBy)
    {
        if (Status != OrderStatus.OutForDelivery)
            return OrderErrors.InvalidStatusTransition;

        return TransitionTo(OrderStatus.Delivered, changedBy);
    }

    public Result<Updated> Complete(string changedBy)
    {
        if (Status != OrderStatus.Delivered)
            return OrderErrors.InvalidStatusTransition;

        return TransitionTo(OrderStatus.Completed, changedBy);
    }

    public Result<Updated> Cancel(string changedBy, string? reason = null)
    {
        if (Status != OrderStatus.Pending && Status != OrderStatus.Accepted)
            return OrderErrors.CannotCancel;

        return TransitionTo(OrderStatus.Cancelled, changedBy);
    }

    private Result<Updated> TransitionTo(OrderStatus newStatus, string changedBy)
    {
        var oldStatus = Status;
        Status = newStatus;

        var history = OrderStatusHistory.Create(Guid.NewGuid(), Id, oldStatus, Status, changedBy);
        if (history.IsError) return history.Errors;

        _statusHistory.Add(history.Value);
        return Result.Updated;
    }
}

