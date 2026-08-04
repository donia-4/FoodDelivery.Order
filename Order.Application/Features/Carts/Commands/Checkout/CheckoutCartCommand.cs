using MediatR;
using Order.Application.Features.Orders.Commands.CreateOrder;
using Order.Domain.Results;

namespace Order.Application.Features.Carts.Commands.Checkout;

public sealed record CheckoutCartCommand(
    Guid CustomerId,
    Guid AddressId,
    decimal DeliveryFee,
    string? Notes) : IRequest<Result<CreateOrderResult>>;
