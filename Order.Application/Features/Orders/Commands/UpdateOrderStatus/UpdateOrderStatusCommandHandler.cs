using MediatR;
using Microsoft.Extensions.Logging;
using Order.Application.Common.Interfaces.Repositories;
using Order.Application.Common.Interfaces.Services;
using Order.Domain.Orders;
using Order.Domain.Orders.Enums;
using Order.Domain.Results;

namespace Order.Application.Features.Orders.Commands.UpdateOrderStatus;

public sealed class UpdateOrderStatusCommandHandler(
    IOrderRepository orderRepository,
    IOrderStatusHistoryRepository orderStatusHistoryRepository,
    ILogger<UpdateOrderStatusCommandHandler> logger,
    ICacheService cacheService)
    : IRequestHandler<UpdateOrderStatusCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(
        UpdateOrderStatusCommand request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling UpdateOrderStatusCommand for OrderId: {OrderId}, NewStatus: {NewStatus}, ChangedBy: {ChangedBy}",
            request.OrderId, request.NewStatus, request.ChangedBy);

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

        var oldStatus = order.Status;

        Result<Updated> result = request.NewStatus switch
        {
            OrderStatus.Preparing => order.StartPreparing(request.ChangedBy),
            OrderStatus.Ready => order.MarkReady(request.ChangedBy),
            _ => OrderErrors.InvalidStatusTransition
        };

        if (result.IsError)
            return result.Errors;

        var historyResult = OrderStatusHistory.Create(
            Guid.NewGuid(),
            order.Id,
            oldStatus,
            order.Status,
            request.ChangedBy);

        if (historyResult.IsError)
            return historyResult.Errors;

        await orderStatusHistoryRepository.AddAsync(historyResult.Value, cancellationToken);
        await orderStatusHistoryRepository.SaveChangesAsync(cancellationToken);

        orderRepository.Update(order);
        await orderRepository.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Order with Id {OrderId} status updated to {NewStatus} by {ChangedBy}.",
            order.Id, request.NewStatus, request.ChangedBy);

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