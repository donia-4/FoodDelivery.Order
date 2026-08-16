using MediatR;
using Order.Application.Features.Carts.Dtos;
using Order.Domain.Results;

namespace Order.Application.Features.Carts.Commands.RemoveCartItem;

public sealed record RemoveCartItemCommand(Guid CustomerId, Guid CartItemId) : IRequest<Result<CartDto>>;
