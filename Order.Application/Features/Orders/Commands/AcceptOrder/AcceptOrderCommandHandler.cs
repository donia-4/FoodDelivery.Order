using MediatR;
using Microsoft.Extensions.Logging;
using Order.Application.Common.Interfaces.Repositories;
using Order.Application.Common.Interfaces.Services;
using Order.Domain.Orders;
using Order.Domain.Results;

namespace Order.Application.Features.Orders.Commands.AcceptOrder;

public sealed class AcceptOrderCommandHandler(
    IOrderRepository orderRepository,
    ICacheService cacheService,
    ILogger<AcceptOrderCommandHandler> logger)
    : IRequestHandler<AcceptOrderCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(
        AcceptOrderCommand request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Accepting order with Id: {OrderId} by user: {ChangedBy}",
            request.OrderId,
            request.ChangedBy);

        var order = await orderRepository.GetByIdAsync(
            request.OrderId,
            cancellationToken);

        if (order is null)
        {
            logger.LogWarning(
                "Order with Id {OrderId} was not found.",
                request.OrderId);

            return OrderErrors.NotFound;
        }


        var result = order.Accept(request.ChangedBy);
        if (result.IsError)
            return result.Errors;

        orderRepository.Update(order);
        await orderRepository.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Order with Id: {OrderId} has been accepted by user: {ChangedBy}",
            request.OrderId,
            request.ChangedBy);

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