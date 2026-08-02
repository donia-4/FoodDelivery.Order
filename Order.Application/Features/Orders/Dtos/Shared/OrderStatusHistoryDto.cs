using Order.Domain.Orders.Enums;

namespace Order.Application.Features.Orders.Dtos.Shared;

public record OrderStatusHistoryDto(
    OrderStatus OldStatus,
    OrderStatus NewStatus,
    string ChangedBy,
    DateTimeOffset ChangedDate);