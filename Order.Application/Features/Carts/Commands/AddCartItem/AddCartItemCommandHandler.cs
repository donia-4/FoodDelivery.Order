using MediatR;
using Order.Application.Common.Interfaces.Repositories;
using Order.Application.Common.Interfaces.Services;
using Order.Application.Common.Validation;
using Order.Application.Features.Carts.Dtos;
using Order.Domain.Carts;
using Order.Domain.Results;

namespace Order.Application.Features.Carts.Commands.AddCartItem;

public sealed class AddCartItemCommandHandler(
    ICartRepository cartRepository,
    IRestaurantService restaurantService)
    : IRequestHandler<AddCartItemCommand, Result<CartDto>>
{
    public async Task<Result<CartDto>> Handle(AddCartItemCommand request, CancellationToken cancellationToken)
    {
        var restaurantName = await restaurantService.GetRestaurantNameAsync(
            request.RestaurantId, cancellationToken);

        if (restaurantName is null)
            return Domain.Orders.OrderErrors.RestaurantNotFound;

        var validationResult = await MenuItemPriceValidator.ValidateAsync(
            restaurantService,
            request.RestaurantId,
            [(request.MenuItemId, request.Quantity)],
            cancellationToken);

        if (validationResult.IsError)
            return validationResult.Errors;

        var unitPrice = validationResult.Value[0].UnitPrice;

        var cart = await cartRepository.GetByCustomerIdAsync(request.CustomerId, cancellationToken);
        var isNewCart = cart is null;

        if (cart is null)
        {
            var cartResult = Cart.Create(Guid.NewGuid(), request.CustomerId, request.RestaurantId);
            if (cartResult.IsError)
                return cartResult.Errors;

            cart = cartResult.Value;
        }
        else if (cart.RestaurantId != request.RestaurantId)
        {
            // Switching restaurants with items already in the cart is rejected by
            // Cart.ChangeRestaurant (CartErrors.HasItems) - the customer needs to
            // clear their cart first rather than silently mixing restaurants.
            var changeResult = cart.ChangeRestaurant(request.RestaurantId);
            if (changeResult.IsError)
                return changeResult.Errors;
        }

        var itemResult = CartItem.Create(
            Guid.NewGuid(), cart.Id, request.MenuItemId, request.RestaurantId,
            request.Quantity, unitPrice, request.Notes);

        if (itemResult.IsError)
            return itemResult.Errors;

        var addResult = cart.AddItem(itemResult.Value);
        if (addResult.IsError)
            return addResult.Errors;

        if (isNewCart)
            await cartRepository.AddAsync(cart, cancellationToken);
        else
            cartRepository.Update(cart);

        await cartRepository.SaveChangesAsync(cancellationToken);

        return cart.ToDto();
    }
}
