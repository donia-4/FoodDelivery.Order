using MediatR;
using Order.Domain.Results;

namespace Order.Application.Features.Orders.Commands.CancelOrder;

public sealed record CancelOrderCommand(Guid OrderId, Guid CustomerId, string? Reason = null)
    : IRequest<Result<Updated>>;
