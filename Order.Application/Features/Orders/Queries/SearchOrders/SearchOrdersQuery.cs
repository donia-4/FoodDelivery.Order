using MediatR;
using Order.Application.Common.Interfaces;
using Order.Application.Common.Models;
using Order.Application.Features.Orders.Dtos.SearchOrders;
using Order.Domain.Orders.Enums;
using Order.Domain.Results;

namespace Order.Application.Features.Orders.Queries.SearchOrders;

public record SearchOrdersQuery(
    Guid? CustomerId = null,
    Guid? RestaurantId = null,
    string? OrderNumber = null,
    OrderStatus? Status = null,
    DateTimeOffset? FromDate = null,
    DateTimeOffset? ToDate = null,
    int Page = 1,
    int PageSize = 20)
    : ICachedQuery<Result<PaginatedList<OrderSummaryDto>>>
{
    public string CacheKey =>
        $"orders:search:{CustomerId}:{RestaurantId}:{OrderNumber}:{Status}:{FromDate:O}:{ToDate:O}:{Page}:{PageSize}";

    public string[] Tags => new[] { "orders", "orders:search" };
    public TimeSpan Expiration => TimeSpan.FromMinutes(1);
}