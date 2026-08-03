using MediatR;
using Order.Application.Common.Interfaces;
using Order.Application.Features.Orders.Dtos.GetPendingOrders;
using Order.Domain.Results;

namespace Order.Application.Features.Orders.Queries.GetPendingOrders;

public record GetPendingOrdersQuery(Guid RestaurantId)
    : ICachedQuery<Result<List<PendingOrderDto>>>
{
    public string CacheKey => $"orders:pending:{RestaurantId}";
    public string[] Tags => new[] { "orders", $"orders:pending:{RestaurantId}" };
    public TimeSpan Expiration => TimeSpan.FromMinutes(2);
}