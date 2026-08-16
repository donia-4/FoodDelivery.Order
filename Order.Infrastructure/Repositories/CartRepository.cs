using Microsoft.EntityFrameworkCore;
using Order.Application.Common.Interfaces.Repositories;
using Order.Domain.Carts;
using Order.Infrastructure.Data;

namespace Order.Infrastructure.Repositories;

public sealed class CartRepository : ICartRepository
{
    private readonly OrderDbContext _context;

    public CartRepository(OrderDbContext context)
    {
        _context = context;
    }

    public async Task<Cart?> GetByCustomerIdAsync(Guid customerId, CancellationToken ct = default)
    {
        return await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.CustomerId == customerId && !c.IsDeleted, ct);
    }

    public async Task<Cart?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, ct);
    }

    public async Task AddAsync(Cart cart, CancellationToken ct = default)
    {
        await _context.Carts.AddAsync(cart, ct);
    }

    public void Update(Cart cart)
    {
        _context.Carts.Update(cart);
    }

    public void Remove(Cart cart)
    {
        _context.Carts.Remove(cart);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _context.SaveChangesAsync(ct);
    }
    public async Task ClearAsync(Guid cartId, CancellationToken ct = default)
    {
        var items = await _context.CartItems
            .Where(i => i.CartId == cartId)
            .ToListAsync(ct);

        _context.CartItems.RemoveRange(items);
    }
    public async Task AddItemAsync(CartItem item, CancellationToken ct = default)
    {
        await _context.CartItems.AddAsync(item, ct);
    }
}
