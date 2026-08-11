using MediatR;
using Order.Application.Common.Interfaces.Repositories;
using Order.Application.Features.Carts.Dtos;
using Order.Domain.Carts;
using Order.Domain.Results;

namespace Order.Application.Features.Carts.Commands.UpdateCartItemQuantity;

public sealed class UpdateCartItemQuantityCommandHandler(ICartRepository cartRepository)
    : IRequestHandler<UpdateCartItemQuantityCommand, Result<CartDto>>
{
    public async Task<Result<CartDto>> Handle(
        UpdateCartItemQuantityCommand request, CancellationToken cancellationToken)
    {
        var cart = await cartRepository.GetByCustomerIdAsync(request.CustomerId, cancellationToken);

        if (cart is null)
            return CartErrors.NotFound;

        var result = cart.UpdateItemQuantity(request.CartItemId, request.Quantity);
        if (result.IsError)
            return result.Errors;

        cartRepository.Update(cart);
        await cartRepository.SaveChangesAsync(cancellationToken);

        return cart.ToDto();
    }
}
