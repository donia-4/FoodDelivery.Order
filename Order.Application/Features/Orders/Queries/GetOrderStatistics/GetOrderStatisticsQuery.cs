using MediatR;
using Order.Application.Common.Interfaces;
using Order.Application.Features.Orders.Dtos.GetOrderStatistics;
using Order.Domain.Results;

namespace Order.Application.Features.Orders.Queries.GetOrderStatistics;

public record GetOrderStatisticsQuery(
    DateTimeOffset? FromDate = null,
    DateTimeOffset? ToDate = null)
    : ICachedQuery<Result<OrderStatisticsDto>>
{
    public string CacheKey =>
        $"orders:statistics:{FromDate:O}:{ToDate:O}";

    public string[] Tags => new[] { "orders", "orders:statistics" };
    public TimeSpan Expiration => TimeSpan.FromMinutes(5);
}