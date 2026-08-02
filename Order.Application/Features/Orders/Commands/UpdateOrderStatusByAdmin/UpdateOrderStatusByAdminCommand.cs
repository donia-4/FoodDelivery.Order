using MediatR;
using Order.Domain.Orders.Enums;
using Order.Domain.Results;

namespace Order.Application.Features.Orders.Commands.UpdateOrderStatusByAdmin;

public record UpdateOrderStatusByAdminCommand(Guid OrderId, OrderStatus NewStatus, string ChangedBy)
    : IRequest<Result<Updated>>;