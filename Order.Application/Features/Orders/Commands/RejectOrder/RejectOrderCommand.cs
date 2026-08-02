using MediatR;
using Order.Domain.Results;

namespace Order.Application.Features.Orders.Commands.RejectOrder;

public record RejectOrderCommand(Guid OrderId, string ChangedBy, string? Reason = null)
    : IRequest<Result<Updated>>;