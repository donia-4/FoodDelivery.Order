namespace Order.Application.Common.Interfaces.Repositories;

public interface ICartRepository
{
    Task<Domain.Carts.Cart?> GetByCustomerIdAsync(Guid customerId, CancellationToken ct = default);
    Task<Domain.Carts.Cart?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task AddAsync(Domain.Carts.Cart cart, CancellationToken ct = default);
    void Update(Domain.Carts.Cart cart);
    void Remove(Domain.Carts.Cart cart);
    Task ClearAsync(Guid cartId, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
