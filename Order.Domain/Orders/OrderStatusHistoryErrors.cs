using Order.Domain.Results;

namespace Order.Domain.Orders;

public static class OrderStatusHistoryErrors
{
    public static readonly Error InvalidOrder = Error.Validation("OrderStatusHistory.Order.Required", "Order is required.");
    public static readonly Error InvalidChangedBy = Error.Validation("OrderStatusHistory.ChangedBy.Required", "Changed by is required.");
}