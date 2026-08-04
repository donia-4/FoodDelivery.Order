using MediatR;
using Order.Application.Common.Interfaces.Repositories;
using Order.Application.Common.Interfaces.Services;
using Order.Application.Features.Orders.Dtos.GetOrderDetails;
using Order.Application.Features.Orders.Dtos.Shared;
using Order.Domain.Orders;
using Order.Domain.Results;

namespace Order.Application.Features.Orders.Queries.GetOrderById;

public sealed class GetOrderByIdQueryHandler(
    IOrderRepository orderRepository,
    IIdentityService identityService,
    IRestaurantService restaurantService)
    : IRequestHandler<GetOrderByIdQuery, Result<OrderDetailsDto>>
{
    public async Task<Result<OrderDetailsDto>> Handle(
        GetOrderByIdQuery request,
        CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdWithItemsAsync(
            request.Id,
            cancellationToken);

        // Return NotFound (not Forbidden) when the order belongs to someone else,
        // so we don't reveal that an order id exists to a customer who doesn't own it.
        if (order is null || order.CustomerId != request.CustomerId)
            return OrderErrors.NotFound;

        var customer = await identityService.GetUserAsync(order.CustomerId, cancellationToken);
        var restaurantName = await restaurantService.GetRestaurantNameAsync(order.RestaurantId, cancellationToken);

        var dto = new OrderDetailsDto(
            order.Id,
            order.OrderNumber,
            order.CustomerId,
            customer?.Name ?? string.Empty,
            customer?.Phone ?? string.Empty,
            order.RestaurantId,
            restaurantName ?? string.Empty,
            string.Empty,
            order.Status,
            order.SubTotal,
            order.DeliveryFee,
            order.Tax,
            order.Discount,
            order.Total,
            order.Notes,
            order.CreatedAtUtc,
            order.Items.Select(i => new OrderItemDto(
                i.MenuItemId,
                string.Empty,
                i.Quantity,
                i.UnitPrice,
                i.Total
            )).ToList(),
            order.StatusHistory.Select(h => new OrderStatusHistoryDto(
                h.OldStatus,
                h.NewStatus,
                h.ChangedBy,
                h.ChangedDate
            )).ToList()
        );

        return dto;
    }
}
