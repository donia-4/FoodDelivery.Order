using MediatR;
using Order.Application.Common.Interfaces.Repositories;
using Order.Application.Common.Interfaces.Services;
using Order.Domain.Orders;
using Order.Domain.Results;

namespace Order.Application.Features.Orders.Commands.AcceptOrder;

public sealed class AcceptOrderCommandHandler(
    IOrderRepository orderRepository,
    ICacheService cacheService)
    : IRequestHandler<AcceptOrderCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(
        AcceptOrderCommand request,
        CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(
            request.OrderId,
            cancellationToken);

        if (order is null)
            return OrderErrors.NotFound;

        var result = order.Accept(request.ChangedBy);
        if (result.IsError)
            return result.Errors;

        orderRepository.Update(order);
        await orderRepository.SaveChangesAsync(cancellationToken);

        await InvalidateCacheAsync(order, cancellationToken);

        return Result.Updated;
    }

    private async Task InvalidateCacheAsync(Domain.Orders.Order order, CancellationToken ct)
    {
        await cacheService.RemoveByTagAsync($"order:{order.Id}", ct);
        await cacheService.RemoveByTagAsync($"orders:pending:{order.RestaurantId}", ct);
        await cacheService.RemoveByTagAsync("orders:search", ct);
        await cacheService.RemoveByTagAsync("orders:statistics", ct);
    }
}