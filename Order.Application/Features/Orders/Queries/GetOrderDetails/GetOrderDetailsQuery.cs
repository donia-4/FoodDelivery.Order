using MediatR;
using Order.Application.Common.Interfaces;
using Order.Application.Features.Orders.Dtos.GetOrderDetails;
using Order.Domain.Results;

namespace Order.Application.Features.Orders.Queries.GetOrderDetails;

public record GetOrderDetailsQuery(Guid Id)
    : ICachedQuery<Result<OrderDetailsDto>>
{
    public string CacheKey => $"order:{Id}";
    public string[] Tags => new[] { "orders", $"order:{Id}" };
    public TimeSpan Expiration => TimeSpan.FromMinutes(5);
}