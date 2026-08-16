using MediatR;
using Order.Application.Common.Interfaces.Repositories;
using Order.Domain.Results;

namespace Order.Application.Features.Carts.Commands.ClearCart;

public sealed class ClearCartCommandHandler(ICartRepository cartRepository)
    : IRequestHandler<ClearCartCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(ClearCartCommand request, CancellationToken cancellationToken)
    {
        var cart = await cartRepository.GetByCustomerIdAsync(request.CustomerId, cancellationToken);

        if (cart is null)
            return Result.Updated;

        await cartRepository.ClearAsync(cart.Id, cancellationToken);

        cartRepository.Remove(cart);

        await cartRepository.SaveChangesAsync(cancellationToken);

        return Result.Updated;
    }
}
