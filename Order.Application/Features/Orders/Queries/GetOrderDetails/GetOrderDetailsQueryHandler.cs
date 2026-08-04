using MediatR;
using Microsoft.Extensions.Logging;
using Order.Application.Common.Interfaces.Repositories;
using Order.Application.Common.Interfaces.Services;
using Order.Application.Features.Orders.Dtos.GetOrderDetails;
using Order.Application.Features.Orders.Dtos.Shared;
using Order.Domain.Orders;
using Order.Domain.Results;

namespace Order.Application.Features.Orders.Queries.GetOrderDetails;

public sealed class GetOrderDetailsQueryHandler(
    IOrderRepository orderRepository,
    IIdentityService identityService,
    IRestaurantService restaurantService,
    ILogger<GetOrderDetailsQueryHandler> logger)
    : IRequestHandler<GetOrderDetailsQuery, Result<OrderDetailsDto>>
{
    public async Task<Result<OrderDetailsDto>> Handle(
        GetOrderDetailsQuery request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Fetching order details for OrderId: {OrderId}",
            request.Id);

        var order = await orderRepository.GetByIdWithItemsAsync(
            request.Id,
            cancellationToken);

        if (order is null)
        {
            logger.LogWarning(
                "Order with Id {OrderId} was not found.",
                request.Id);

            return OrderErrors.NotFound;
        }

        var customer = await identityService.GetUserAsync(order.CustomerId, cancellationToken);
        if (customer is null)
        {
            logger.LogWarning(
                "Customer with Id {CustomerId} for Order {OrderId} was not found in Identity Service.",
                order.CustomerId,
                order.Id);
        }

        var restaurantName = await restaurantService.GetRestaurantNameAsync(order.RestaurantId, cancellationToken);
        if (string.IsNullOrWhiteSpace(restaurantName))
        {
            logger.LogWarning(
                "Restaurant with Id {RestaurantId} for Order {OrderId} was not found in Restaurant Service.",
                order.RestaurantId,
                order.Id);
        }

        if (order.Items.Count == 0)
        {
            logger.LogWarning(
                "Order {OrderId} has no items.",
                order.Id);
        }

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

        logger.LogInformation(
            "Order {OrderId} retrieved successfully.",
            order.Id);

        return dto;
    }
}