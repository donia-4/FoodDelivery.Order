using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Order.API.Extensions;
using Order.Application.Features.Carts.Commands.AddCartItem;
using Order.Application.Features.Carts.Commands.Checkout;
using Order.Application.Features.Carts.Commands.ClearCart;
using Order.Application.Features.Carts.Commands.RemoveCartItem;
using Order.Application.Features.Carts.Commands.UpdateCartItemQuantity;
using Order.Application.Features.Carts.Queries.GetCart;

namespace Order.API.Controllers;

[Authorize(Roles = "Customer")]
[Route("customer/cart")]
public class CartController : ApiController
{
    private readonly ISender _sender;

    public CartController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IActionResult> GetCart(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetCartQuery(User.GetUserId()), cancellationToken);

        return result.IsSuccess
            ? OkEnvelope(result.Value)
            : Problem(result.Errors);
    }

    [HttpPost("items")]
    public async Task<IActionResult> AddItem(AddCartItemRequest request, CancellationToken cancellationToken)
    {
        var command = new AddCartItemCommand(
            User.GetUserId(), request.RestaurantId, request.MenuItemId, request.Quantity, request.Notes);

        var result = await _sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? OkEnvelope(result.Value)
            : Problem(result.Errors);
    }

    [HttpPut("items/{cartItemId:guid}")]
    public async Task<IActionResult> UpdateItemQuantity(
        Guid cartItemId, UpdateCartItemQuantityRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateCartItemQuantityCommand(User.GetUserId(), cartItemId, request.Quantity);
        var result = await _sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? OkEnvelope(result.Value)
            : Problem(result.Errors);
    }

    [HttpDelete("items/{cartItemId:guid}")]
    public async Task<IActionResult> RemoveItem(Guid cartItemId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new RemoveCartItemCommand(User.GetUserId(), cartItemId), cancellationToken);

        return result.IsSuccess
            ? OkEnvelope(result.Value)
            : Problem(result.Errors);
    }

    [HttpDelete]
    public async Task<IActionResult> ClearCart(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ClearCartCommand(User.GetUserId()), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : Problem(result.Errors);
    }

    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout(CheckoutCartRequest request, CancellationToken cancellationToken)
    {
        var command = new CheckoutCartCommand(
            User.GetUserId(), request.AddressId, request.DeliveryFee, request.Notes);

        var result = await _sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? CreatedEnvelope(result.Value, "Order created successfully")
            : Problem(result.Errors);
    }
}

public sealed record AddCartItemRequest(Guid RestaurantId, Guid MenuItemId, int Quantity, string? Notes);
public sealed record UpdateCartItemQuantityRequest(int Quantity);
public sealed record CheckoutCartRequest(Guid AddressId, decimal DeliveryFee, string? Notes);
