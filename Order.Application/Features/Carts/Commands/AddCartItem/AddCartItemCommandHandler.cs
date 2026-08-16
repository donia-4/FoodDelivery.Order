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
            var changeResult = cart.ChangeRestaurant(request.RestaurantId);
            if (changeResult.IsError)
                return changeResult.Errors;
        }

        var existingItem = cart.Items.FirstOrDefault(i => i.MenuItemId == request.MenuItemId);

        if (existingItem is not null)
        {
            var updateResult = existingItem.UpdateQuantity(existingItem.Quantity + request.Quantity);
            if (updateResult.IsError)
                return updateResult.Errors;
        }
        else
        {
            var itemResult = CartItem.Create(
                Guid.NewGuid(), cart.Id, request.MenuItemId, request.RestaurantId,
                request.Quantity, unitPrice, request.Notes);

            if (itemResult.IsError)
                return itemResult.Errors;

            if (itemResult.Value.RestaurantId != cart.RestaurantId)
                return CartErrors.ItemFromDifferentRestaurant;

            cart.AddItem(itemResult.Value);

            await cartRepository.AddItemAsync(itemResult.Value, cancellationToken);
        }

        if (isNewCart)
            await cartRepository.AddAsync(cart, cancellationToken);

        await cartRepository.SaveChangesAsync(cancellationToken);

        return cart.ToDto();
    }
}