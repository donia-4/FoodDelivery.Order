using Order.Application.Common.Models;
using Order.Application.Features.Orders.Dtos.GetOrderStatistics;
using Order.Domain.Orders;
using Order.Domain.Orders.Enums;

namespace Order.Application.Common.Interfaces.Repositories;

public interface IOrderRepository
{
    Task<Domain.Orders.Order?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Domain.Orders.Order?> GetByIdWithItemsAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Domain.Orders.Order>> GetPendingByRestaurantAsync(Guid restaurantId, CancellationToken ct = default);

    Task<PaginatedList<Domain.Orders.Order>> SearchAsync(
        Guid? customerId,
        Guid? restaurantId,
        string? orderNumber,
        OrderStatus? status,
        DateTimeOffset? fromDate,
        DateTimeOffset? toDate,
        int page,
        int pageSize,
        CancellationToken ct = default);

    Task<OrderStatisticsDto> GetStatisticsAsync(
        DateTimeOffset? fromDate,
        DateTimeOffset? toDate,
        CancellationToken ct = default);

    Task AddAsync(Domain.Orders.Order order, CancellationToken ct = default);
    void Update(Domain.Orders.Order order);
    Task SaveChangesAsync(CancellationToken ct = default);
}