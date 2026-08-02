using MediatR;
using Order.Application.Common.Interfaces.Repositories;
using Order.Application.Common.Interfaces.Services;
using Order.Domain.Orders;
using Order.Domain.Orders.Enums;
using Order.Domain.Results;

namespace Order.Application.Features.Orders.Commands.UpdateOrderStatusByAdmin;

public sealed class UpdateOrderStatusByAdminCommandHandler(
    IOrderRepository orderRepository,
    ICacheService cacheService)
    : IRequestHandler<UpdateOrderStatusByAdminCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(
        UpdateOrderStatusByAdminCommand request,
        CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(
            request.OrderId,
            cancellationToken);

        if (order is null)
            return OrderErrors.NotFound;

        Result<Updated> result = request.NewStatus switch
        {
            OrderStatus.Accepted => order.Accept(request.ChangedBy),
            OrderStatus.Rejected => order.Reject(request.ChangedBy),
            OrderStatus.Preparing => order.StartPreparing(request.ChangedBy),
            OrderStatus.Ready => order.MarkReady(request.ChangedBy),
            OrderStatus.OutForDelivery => order.MarkOutForDelivery(request.ChangedBy),
            OrderStatus.Delivered => order.MarkDelivered(request.ChangedBy),
            OrderStatus.Completed => order.Complete(request.ChangedBy),
            OrderStatus.Cancelled => order.Cancel(request.ChangedBy),
            _ => OrderErrors.InvalidStatusTransition
        };

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