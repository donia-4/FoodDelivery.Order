using MediatR;
using Order.Application.Common.Interfaces.Repositories;
using Order.Application.Features.Orders.Dtos.GetOrderStatistics;
using Order.Domain.Results;

namespace Order.Application.Features.Orders.Queries.GetOrderStatistics;

public sealed class GetOrderStatisticsQueryHandler(IOrderRepository orderRepository)
    : IRequestHandler<GetOrderStatisticsQuery, Result<OrderStatisticsDto>>
{
    public async Task<Result<OrderStatisticsDto>> Handle(
        GetOrderStatisticsQuery request,
        CancellationToken cancellationToken)
    {
        var statistics = await orderRepository.GetStatisticsAsync(
            request.FromDate,
            request.ToDate,
            cancellationToken);

        return statistics;
    }
}