using MediatR;
using Order.Application.Common.Interfaces.Repositories;
using Order.Application.Features.Carts.Dtos;
using Order.Domain.Results;

namespace Order.Application.Features.Carts.Queries.GetCart;

public sealed class GetCartQueryHandler(ICartRepository cartRepository)
    : IRequestHandler<GetCartQuery, Result<CartDto>>
{
    public async Task<Result<CartDto>> Handle(GetCartQuery request, CancellationToken cancellationToken)
    {
        var cart = await cartRepository.GetByCustomerIdAsync(request.CustomerId, cancellationToken);

        if (cart is null)
            return new CartDto(Guid.Empty, request.CustomerId, Guid.Empty, [], 0m);

        return cart.ToDto();
    }
}
