using MediatR;
using Order.Application.Features.Carts.Dtos;
using Order.Domain.Results;

namespace Order.Application.Features.Carts.Commands.AddCartItem;

public sealed record AddCartItemCommand(
    Guid CustomerId,
    Guid RestaurantId,
    Guid MenuItemId,
    int Quantity,
    string? Notes) : IRequest<Result<CartDto>>;
