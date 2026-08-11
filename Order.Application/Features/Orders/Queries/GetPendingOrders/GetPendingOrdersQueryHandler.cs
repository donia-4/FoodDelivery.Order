using MediatR;
using Microsoft.Extensions.Logging;
using Order.Application.Common.Interfaces.Repositories;
using Order.Application.Features.Orders.Dtos.GetPendingOrders;
using Order.Domain.Results;

namespace Order.Application.Features.Orders.Queries.GetPendingOrders;

public sealed class GetPendingOrdersQueryHandler(IOrderRepository orderRepository
    ,ILogger<GetPendingOrdersQueryHandler> logger)
    : IRequestHandler<GetPendingOrdersQuery, Result<List<PendingOrderDto>>>
{
    public async Task<Result<List<PendingOrderDto>>> Handle(
        GetPendingOrdersQuery request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Fetching pending orders for RestaurantId: {RestaurantId}",
            request.RestaurantId);

        var orders = await orderRepository.GetPendingByRestaurantAsync(
            request.RestaurantId,
            cancellationToken);

        var dtos = orders.Select(o => new PendingOrderDto(
            o.Id,
            o.OrderNumber,
            o.CustomerId,
            o.SubTotal,
            o.Total,
            o.CreatedAtUtc
        )).ToList();

        logger.LogInformation(
            "Fetched {Count} pending orders for RestaurantId: {RestaurantId}",
            dtos.Count,
            request.RestaurantId);

        return dtos;
    }
}