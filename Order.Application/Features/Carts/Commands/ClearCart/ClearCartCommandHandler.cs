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

        cart.Clear();

        cartRepository.Update(cart);
        await cartRepository.SaveChangesAsync(cancellationToken);

        return Result.Updated;
    }
}
