using MediatR;
using Order.Domain.Results;

namespace Order.Application.Features.Orders.Commands.AcceptOrder;

public record AcceptOrderCommand(Guid OrderId, string ChangedBy)
    : IRequest<Result<Updated>>;