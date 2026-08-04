using MediatR;
using Order.Application.Common.Interfaces.Messaging;
using Order.Application.Common.Interfaces.Repositories;
using Order.Application.Common.Messages;
using Order.Application.Features.Orders.Commands.CreateOrder;
using Order.Application.Features.Orders.Events;
using Order.Domain.Carts;
using Order.Domain.Results;

namespace Order.Application.Features.Carts.Commands.Checkout;

public sealed class CheckoutCartCommandHandler(
    ICartRepository cartRepository,
    IOrderRepository orderRepository,
    IOutbox outbox)
    : IRequestHandler<CheckoutCartCommand, Result<CreateOrderResult>>
{
    private const decimal TaxRate = 0.14m;

    public async Task<Result<CreateOrderResult>> Handle(
        CheckoutCartCommand request, CancellationToken cancellationToken)
    {
        var cart = await cartRepository.GetByCustomerIdAsync(request.CustomerId, cancellationToken);

        if (cart is null)
            return CartErrors.NotFound;

        var orderId = Guid.NewGuid();
        var orderNumber = GenerateOrderNumber();

        // Checkout builds the Order (with items, so SubTotal is already accurate) but
        // doesn't know the tax rate - it's computed from the resulting SubTotal below,
        // same as the direct CreateOrder flow.
        var checkoutResult = cart.Checkout(
            orderId, orderNumber, request.AddressId, request.DeliveryFee, tax: 0m, discount: 0m, request.Notes);

        if (checkoutResult.IsError)
            return checkoutResult.Errors;

        var order = checkoutResult.Value;

        var tax = Math.Round(order.SubTotal * TaxRate, 2);
        var taxResult = order.Update(tax: tax);
        if (taxResult.IsError)
            return taxResult.Errors;

        await orderRepository.AddAsync(order, cancellationToken);
        cartRepository.Remove(cart);

        await outbox.AddAsync(
            new OrderCreatedEvent(order.Id, order.CustomerId, order.RestaurantId, order.OrderNumber, order.Total),
            RoutingKeys.OrderCreated,
            cancellationToken);

        // Cart and Order live in the same DbContext instance for this scope, so one
        // SaveChanges call persists both the new order and the cart removal.
        await orderRepository.SaveChangesAsync(cancellationToken);

        return new CreateOrderResult(order.Id, order.OrderNumber, order.Total, order.Status.ToString());
    }

    private static string GenerateOrderNumber() =>
        $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}";
}
