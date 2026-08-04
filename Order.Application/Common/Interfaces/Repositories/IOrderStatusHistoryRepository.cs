using Microsoft.EntityFrameworkCore;
using Order.Domain.Orders;

namespace Order.Application.Common.Interfaces.Repositories;

public interface IOrderStatusHistoryRepository
{
    Task AddAsync(OrderStatusHistory history, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}