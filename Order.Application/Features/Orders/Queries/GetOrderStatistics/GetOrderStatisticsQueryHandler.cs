using MediatR;
using Microsoft.Extensions.Logging;
using Order.Application.Common.Interfaces.Repositories;
using Order.Application.Features.Orders.Dtos.GetOrderStatistics;
using Order.Domain.Results;

namespace Order.Application.Features.Orders.Queries.GetOrderStatistics;

public sealed class GetOrderStatisticsQueryHandler(IOrderRepository orderRepository
    ,ILogger<GetOrderStatisticsQueryHandler> logger)
    : IRequestHandler<GetOrderStatisticsQuery, Result<OrderStatisticsDto>>
{
    public async Task<Result<OrderStatisticsDto>> Handle(
        GetOrderStatisticsQuery request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Fetching order statistics from {FromDate} to {ToDate}",
            request.FromDate,
            request.ToDate);

        var statistics = await orderRepository.GetStatisticsAsync(
            request.FromDate,
            request.ToDate,
            cancellationToken);

        logger.LogInformation(
                "Order statistics retrieved successfully for date range {FromDate} to {ToDate}",
                request.FromDate,
                request.ToDate);

        return statistics;
    }
}