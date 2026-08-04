using Order.Application.Common.Interfaces.Repositories;
using Order.Domain.Orders;
using Order.Infrastructure.Data;

namespace Order.Infrastructure.Repositories;

public sealed class OrderStatusHistoryRepository : IOrderStatusHistoryRepository
{
    private readonly OrderDbContext _context;

    public OrderStatusHistoryRepository(OrderDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(OrderStatusHistory history, CancellationToken ct = default)
    {
        await _context.OrderStatusHistories.AddAsync(history, ct);
    }
    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _context.SaveChangesAsync(ct);
    }

}