using MediatR;
using Order.Domain.Results;

namespace Order.Application.Features.Orders.Commands.CreateOrder;

public sealed record CreateOrderCommand(
    Guid CustomerId,
    Guid RestaurantId,
    Guid AddressId,
    decimal DeliveryFee,
    string? Notes,
    List<CreateOrderItem> Items) : IRequest<Result<CreateOrderResult>>;

public sealed record CreateOrderItem(Guid MenuItemId, int Quantity);

public sealed record CreateOrderResult(Guid OrderId, string OrderNumber, decimal Total, string Status);
