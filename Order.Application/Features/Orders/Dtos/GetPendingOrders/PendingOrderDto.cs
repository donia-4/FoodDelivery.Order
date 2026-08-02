namespace Order.Application.Features.Orders.Dtos.GetPendingOrders;

public record PendingOrderDto(
    Guid Id,
    string OrderNumber,
    Guid CustomerId,
    decimal SubTotal,
    decimal Total,
    DateTimeOffset CreatedAtUtc);