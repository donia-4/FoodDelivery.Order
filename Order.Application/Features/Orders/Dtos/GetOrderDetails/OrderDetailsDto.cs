using Order.Domain.Orders.Enums;
using Order.Application.Features.Orders.Dtos.Shared;

namespace Order.Application.Features.Orders.Dtos.GetOrderDetails;

public record OrderDetailsDto(
    Guid Id,
    string OrderNumber,
    Guid CustomerId,
    string CustomerName,
    string CustomerPhone,
    Guid RestaurantId,
    string RestaurantName,
    string DeliveryAddress,
    OrderStatus Status,
    decimal SubTotal,
    decimal DeliveryFee,
    decimal Tax,
    decimal Discount,
    decimal Total,
    string? Notes,
    DateTimeOffset CreatedAtUtc,
    IReadOnlyList<OrderItemDto> Items,
    IReadOnlyList<OrderStatusHistoryDto> StatusHistory);