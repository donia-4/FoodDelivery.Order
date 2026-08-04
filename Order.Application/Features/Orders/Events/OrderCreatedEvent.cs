namespace Order.Application.Features.Orders.Events;

public sealed record OrderCreatedEvent(
    Guid OrderId,
    Guid CustomerId,
    Guid RestaurantId,
    string OrderNumber,
    decimal Total);
