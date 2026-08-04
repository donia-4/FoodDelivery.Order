using Microsoft.EntityFrameworkCore;
using Order.Application.Common.Interfaces.Repositories;
using Order.Application.Common.Models;
using Order.Application.Features.Orders.Dtos.GetOrderStatistics;
using Order.Domain.Orders;
using Order.Domain.Orders.Enums;
using Order.Infrastructure.Data;

namespace Order.Infrastructure.Repositories;

public sealed class OrderRepository : IOrderRepository
{
    private readonly OrderDbContext _context;

    public OrderRepository(OrderDbContext context)
    {
        _context = context;
    }

    public async Task<Domain.Orders.Order?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted, ct);
    }

    public async Task<Domain.Orders.Order?> GetByIdWithItemsAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Orders
            .AsSplitQuery()
            .Include(o => o.Items)
            .Include(o => o.StatusHistory)
            .FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted, ct);
    }

    public async Task<IReadOnlyList<Domain.Orders.Order>> GetPendingByRestaurantAsync(
        Guid restaurantId, CancellationToken ct = default)
    {
        return await _context.Orders
            .AsNoTracking()
            .Where(o => o.RestaurantId == restaurantId
                && o.Status == OrderStatus.Pending
                && !o.IsDeleted)
            .OrderByDescending(o => o.CreatedAtUtc)
            .ToListAsync(ct);
    }

    public async Task<PaginatedList<Domain.Orders.Order>> SearchAsync(
        Guid? customerId,
        Guid? restaurantId,
        string? orderNumber,
        OrderStatus? status,
        DateTimeOffset? fromDate,
        DateTimeOffset? toDate,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = _context.Orders
            .AsNoTracking()
            .Where(o => !o.IsDeleted);

        if (customerId.HasValue)
            query = query.Where(o => o.CustomerId == customerId.Value);

        if (restaurantId.HasValue)
            query = query.Where(o => o.RestaurantId == restaurantId.Value);

        if (!string.IsNullOrWhiteSpace(orderNumber))
            query = query.Where(o => o.OrderNumber.Contains(orderNumber));

        if (status.HasValue)
            query = query.Where(o => o.Status == status.Value);

        if (fromDate.HasValue)
            query = query.Where(o => o.CreatedAtUtc >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(o => o.CreatedAtUtc <= toDate.Value);

        query = query.OrderByDescending(o => o.CreatedAtUtc);

        return await PaginatedList<Domain.Orders.Order>.CreateAsync(query, page, pageSize, ct);
    }

    public async Task<OrderStatisticsDto> GetStatisticsAsync(
        DateTimeOffset? fromDate,
        DateTimeOffset? toDate,
        CancellationToken ct = default)
    {
        var query = _context.Orders
            .AsNoTracking()
            .Where(o => !o.IsDeleted);

        if (fromDate.HasValue)
            query = query.Where(o => o.CreatedAtUtc >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(o => o.CreatedAtUtc <= toDate.Value);

        var totalOrders = await query.CountAsync(ct);
        var completedOrders = await query.CountAsync(o => o.Status == OrderStatus.Completed, ct);
        var cancelledOrders = await query.CountAsync(o => o.Status == OrderStatus.Cancelled, ct);
        var totalSales = await query
            .Where(o => o.Status == OrderStatus.Completed)
            .SumAsync(o => (decimal?)o.Total, ct) ?? 0;

        var acceptedToReadyPairs = await _context.OrderStatusHistories
            .AsNoTracking()
            .Where(h => h.NewStatus == OrderStatus.Ready)
            .Join(
                _context.OrderStatusHistories.Where(h2 => h2.NewStatus == OrderStatus.Accepted),
                ready => ready.OrderId,
                accepted => accepted.OrderId,
                (ready, accepted) => new { Ready = ready, Accepted = accepted })
            .Select(x => (x.Ready.ChangedDate - x.Accepted.ChangedDate).TotalMinutes)
            .ToListAsync(ct);

        var avgPrepTime = acceptedToReadyPairs.Count > 0
            ? acceptedToReadyPairs.Average()
            : 0;

        return new OrderStatisticsDto(
            totalOrders,
            completedOrders,
            cancelledOrders,
            totalSales,
            avgPrepTime);
    }

    public async Task AddAsync(Domain.Orders.Order order, CancellationToken ct = default)
    {
        await _context.Orders.AddAsync(order, ct);
    }

    public void Update(Domain.Orders.Order order)
    {
        _context.Orders.Update(order);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _context.SaveChangesAsync(ct);
    }
}