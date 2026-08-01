using Order.Domain.Common;
using Order.Domain.Orders.Enums;
using Order.Domain.Results;

namespace Order.Domain.Orders;

public sealed class OrderStatusHistory : AuditableEntity
{
    public Guid OrderId { get; private set; }
    public OrderStatus OldStatus { get; private set; }
    public OrderStatus NewStatus { get; private set; }
    public string ChangedBy { get; private set; } = string.Empty;
    public DateTimeOffset ChangedDate { get; private set; }

    private OrderStatusHistory() { }

    private OrderStatusHistory(Guid id, Guid orderId, OrderStatus oldStatus, OrderStatus newStatus, string changedBy, DateTimeOffset changedDate)
        : base(id)
    {
        OrderId = orderId;
        OldStatus = oldStatus;
        NewStatus = newStatus;
        ChangedBy = changedBy;
        ChangedDate = changedDate;
    }

    public static Result<OrderStatusHistory> Create(Guid id, Guid orderId, OrderStatus oldStatus, OrderStatus newStatus, string changedBy)
    {
        if (orderId == Guid.Empty) return OrderStatusHistoryErrors.InvalidOrder;
        if (string.IsNullOrWhiteSpace(changedBy)) return OrderStatusHistoryErrors.InvalidChangedBy;

        return new OrderStatusHistory(id, orderId, oldStatus, newStatus, changedBy.Trim(), DateTimeOffset.UtcNow);
    }
}
