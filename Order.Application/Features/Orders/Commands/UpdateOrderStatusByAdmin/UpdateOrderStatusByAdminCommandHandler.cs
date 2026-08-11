using MediatR;
using Microsoft.Extensions.Logging;
using Order.Application.Common.Interfaces.Repositories;
using Order.Application.Common.Interfaces.Services;
using Order.Domain.Orders;
using Order.Domain.Orders.Enums;
using Order.Domain.Results;

namespace Order.Application.Features.Orders.Commands.UpdateOrderStatusByAdmin;

public sealed class UpdateOrderStatusByAdminCommandHandler(
    IOrderRepository orderRepository,
    IOrderStatusHistoryRepository orderStatusHistoryRepository,
    ICacheService cacheService,
    ICurrentUserService currentUserService,
    ILogger<UpdateOrderStatusByAdminCommandHandler> logger)
    : IRequestHandler<UpdateOrderStatusByAdminCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(
        UpdateOrderStatusByAdminCommand request,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;

        logger.LogInformation(
            "Handling UpdateOrderStatusByAdminCommand for OrderId: {OrderId}, NewStatus: {NewStatus}, ChangedBy: {ChangedBy}",
            request.OrderId, request.NewStatus, userId);

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
            OrderStatus.Accepted => order.Accept(userId.Value.ToString()),
            OrderStatus.Rejected => order.Reject(userId.Value.ToString()),
            OrderStatus.Preparing => order.StartPreparing(userId.Value.ToString()),
            OrderStatus.Ready => order.MarkReady(userId.Value.ToString()),
            OrderStatus.OutForDelivery => order.MarkOutForDelivery(userId.Value.ToString()),
            OrderStatus.Delivered => order.MarkDelivered(userId.Value.ToString()),
            OrderStatus.Completed => order.Complete(userId.Value.ToString()),
            OrderStatus.Cancelled => order.Cancel(userId.Value.ToString()),
            _ => OrderErrors.InvalidStatusTransition
        };

        if (result.IsError)
            return result.Errors;

        var historyResult = OrderStatusHistory.Create(
            Guid.NewGuid(),
            order.Id,
            oldStatus,
            order.Status,
            userId.Value.ToString());

        if (historyResult.IsError)
            return historyResult.Errors;

        await orderStatusHistoryRepository.AddAsync(
            historyResult.Value,
            cancellationToken);

        orderRepository.Update(order);

        await orderRepository.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Order with Id {OrderId} status updated to {NewStatus} by {ChangedBy}.",
            order.Id, request.NewStatus, userId);

        await InvalidateCacheAsync(order, cancellationToken);

        return Result.Updated;
    }

    private async Task InvalidateCacheAsync(
        Domain.Orders.Order order,
        CancellationToken ct)
    {
        await cacheService.RemoveByTagAsync($"order:{order.Id}", ct);
        await cacheService.RemoveByTagAsync($"orders:pending:{order.RestaurantId}", ct);
        await cacheService.RemoveByTagAsync("orders:search", ct);
        await cacheService.RemoveByTagAsync("orders:statistics", ct);
    }
}