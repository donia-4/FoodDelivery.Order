using MediatR;
using Order.Domain.Results;

namespace Order.Application.Features.Carts.Commands.ClearCart;

public sealed record ClearCartCommand(Guid CustomerId) : IRequest<Result<Updated>>;
