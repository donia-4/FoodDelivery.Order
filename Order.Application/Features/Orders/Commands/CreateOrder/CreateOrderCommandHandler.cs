using MediatR;
using Order.Application.Common.Interfaces.Messaging;
using Order.Application.Common.Interfaces.Repositories;
using Order.Application.Common.Interfaces.Services;
using Order.Application.Common.Messages;
using Order.Application.Common.Validation;
using Order.Application.Features.Orders.Events;
using Order.Domain.Orders;
using Order.Domain.Results;

namespace Order.Application.Features.Orders.Commands.CreateOrder;

public sealed class CreateOrderCommandHandler(
    IOrderRepository orderRepository,
    IRestaurantService restaurantService,
    IOutbox outbox)
    : IRequestHandler<CreateOrderCommand, Result<CreateOrderResult>>
{
    private const decimal TaxRate = 0.14m;

    public async Task<Result<CreateOrderResult>> Handle(
        CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var restaurantName = await restaurantService.GetRestaurantNameAsync(
            request.RestaurantId, cancellationToken);

        if (restaurantName is null)
            return OrderErrors.RestaurantNotFound;

        // Every item's price/availability is verified against the restaurant service
        // here - the client's own numbers are never used for the actual charge.
        var validationResult = await MenuItemPriceValidator.ValidateAsync(
            restaurantService,
            request.RestaurantId,
            request.Items.Select(i => (i.MenuItemId, i.Quantity)),
            cancellationToken);

        if (validationResult.IsError)
            return validationResult.Errors;

        var validatedItems = validationResult.Value;

        var orderId = Guid.NewGuid();
        var orderNumber = GenerateOrderNumber();

        // Create with subTotal/tax at 0 - items are added below via AddItem, which
        // is what actually accumulates SubTotal (passing a precomputed subtotal here
        // as well would double count, same issue as Cart.Checkout).
        var orderResult = Domain.Orders.Order.Create(
            orderId, orderNumber, request.CustomerId, request.RestaurantId, request.AddressId,
            subTotal: 0m, request.DeliveryFee, tax: 0m, discount: 0m, request.Notes);

        if (orderResult.IsError)
            return orderResult.Errors;

        var order = orderResult.Value;

        foreach (var item in validatedItems)
        {
            var itemResult = OrderItem.Create(
                Guid.NewGuid(), order.Id, item.MenuItemId, item.Quantity, item.UnitPrice);

            if (itemResult.IsError)
                return itemResult.Errors;

            var addResult = order.AddItem(itemResult.Value);
            if (addResult.IsError)
                return addResult.Errors;
        }

        var tax = Math.Round(order.SubTotal * TaxRate, 2);
        var taxResult = order.Update(tax: tax);
        if (taxResult.IsError)
            return taxResult.Errors;

        await orderRepository.AddAsync(order, cancellationToken);

        await outbox.AddAsync(
            new OrderCreatedEvent(order.Id, order.CustomerId, order.RestaurantId, order.OrderNumber, order.Total),
            RoutingKeys.OrderCreated,
            cancellationToken);

        await orderRepository.SaveChangesAsync(cancellationToken);

        return new CreateOrderResult(order.Id, order.OrderNumber, order.Total, order.Status.ToString());
    }

    private static string GenerateOrderNumber() =>
        $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}";
}

