using MediatR;
using Order.Application.Features.Carts.Dtos;
using Order.Domain.Results;

namespace Order.Application.Features.Carts.Queries.GetCart;

public sealed record GetCartQuery(Guid CustomerId) : IRequest<Result<CartDto>>;
