using MediatR;
using Order.Application.Features.Carts.Dtos;
using Order.Domain.Results;

namespace Order.Application.Features.Carts.Commands.UpdateCartItemQuantity;

public sealed record UpdateCartItemQuantityCommand(
    Guid CustomerId, Guid CartItemId, int Quantity) : IRequest<Result<CartDto>>;
