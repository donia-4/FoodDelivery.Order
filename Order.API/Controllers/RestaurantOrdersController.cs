using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Order.Application.Features.Orders.Commands.AcceptOrder;
using Order.Application.Features.Orders.Commands.RejectOrder;
using Order.Application.Features.Orders.Commands.UpdateOrderStatus;
using Order.Application.Features.Orders.Queries.GetOrderDetails;
using Order.Application.Features.Orders.Queries.GetPendingOrders;
using Order.Domain.Orders.Enums;

namespace Order.API.Controllers;

[Authorize(Roles = "RestaurantOwner")]
[Route("restaurant/orders")]
public class RestaurantOrdersController : ApiController
{
    private readonly ISender _sender;

    public RestaurantOrdersController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("pending")]
    public async Task<IActionResult> GetPending([FromQuery] Guid restaurantId)
    {
        var result = await _sender.Send(new GetPendingOrdersQuery(restaurantId));

        return result.IsSuccess
            ? OkEnvelope(result.Value)
            : Problem(result.Errors);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetDetails(Guid id)
    {
        var result = await _sender.Send(new GetOrderDetailsQuery(id));

        return result.IsSuccess
            ? OkEnvelope(result.Value)
            : Problem(result.Errors);
    }

    [HttpPost("accept/{id:guid}")]
    public async Task<IActionResult> Accept(Guid id)
    {
        var result = await _sender.Send(new AcceptOrderCommand(id, User.Identity!.Name!));

        return result.IsSuccess
            ? OkEnvelope(result.Value)
            : Problem(result.Errors);
    }

    [HttpPost("reject/{id:guid}")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectRequest request)
    {
        var result = await _sender.Send(
            new RejectOrderCommand(id, User.Identity!.Name!, request.Reason));

        return result.IsSuccess
            ? OkEnvelope(result.Value)
            : Problem(result.Errors);
    }

    [HttpPost("prepare/{id:guid}")]
    public async Task<IActionResult> Prepare(Guid id)
    {
        var result = await _sender.Send(
            new UpdateOrderStatusCommand(id, OrderStatus.Preparing, User.Identity!.Name!));

        return result.IsSuccess
            ? OkEnvelope(result.Value)
            : Problem(result.Errors);
    }

    [HttpPost("ready/{id:guid}")]
    public async Task<IActionResult> Ready(Guid id)
    {
        var result = await _sender.Send(
            new UpdateOrderStatusCommand(id, OrderStatus.Ready, User.Identity!.Name!));

        return result.IsSuccess
            ? OkEnvelope(result.Value)
            : Problem(result.Errors);
    }
}

public record RejectRequest(string? Reason);