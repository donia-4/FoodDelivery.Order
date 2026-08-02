using Order.Domain.Orders.Enums;

namespace Order.Application.Features.Orders.Dtos.SearchOrders;

public record OrderSummaryDto(
    Guid Id,
    string OrderNumber,
    Guid CustomerId,
    Guid RestaurantId,
    OrderStatus Status,
    decimal Total,
    DateTimeOffset CreatedAtUtc);