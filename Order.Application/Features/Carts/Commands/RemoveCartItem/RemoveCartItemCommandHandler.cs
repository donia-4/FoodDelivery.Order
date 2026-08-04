using MediatR;
using Order.Application.Common.Interfaces.Repositories;
using Order.Application.Features.Carts.Dtos;
using Order.Domain.Carts;
using Order.Domain.Results;

namespace Order.Application.Features.Carts.Commands.RemoveCartItem;

public sealed class RemoveCartItemCommandHandler(ICartRepository cartRepository)
    : IRequestHandler<RemoveCartItemCommand, Result<CartDto>>
{
    public async Task<Result<CartDto>> Handle(RemoveCartItemCommand request, CancellationToken cancellationToken)
    {
        var cart = await cartRepository.GetByCustomerIdAsync(request.CustomerId, cancellationToken);

        if (cart is null)
            return CartErrors.NotFound;

        var result = cart.RemoveItem(request.CartItemId);
        if (result.IsError)
            return result.Errors;

        cartRepository.Update(cart);
        await cartRepository.SaveChangesAsync(cancellationToken);

        return cart.ToDto();
    }
}
