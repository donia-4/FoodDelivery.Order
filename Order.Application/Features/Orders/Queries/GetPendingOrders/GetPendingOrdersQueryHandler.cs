using MediatR;
using Order.Application.Common.Interfaces.Repositories;
using Order.Application.Features.Orders.Dtos.GetPendingOrders;
using Order.Domain.Results;

namespace Order.Application.Features.Orders.Queries.GetPendingOrders;

public sealed class GetPendingOrdersQueryHandler(IOrderRepository orderRepository)
    : IRequestHandler<GetPendingOrdersQuery, Result<List<PendingOrderDto>>>
{
    public async Task<Result<List<PendingOrderDto>>> Handle(
        GetPendingOrdersQuery request,
        CancellationToken cancellationToken)
    {
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

        return dtos;
    }
}