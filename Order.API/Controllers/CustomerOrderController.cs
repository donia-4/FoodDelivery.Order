using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Order.API.Extensions;
using Order.Application.Features.Orders.Commands.CancelOrder;
using Order.Application.Features.Orders.Commands.CreateOrder;
using Order.Application.Features.Orders.Queries.GetCustomerOrderHistory;
using Order.Application.Features.Orders.Queries.GetOrderById;

namespace Order.API.Controllers;

[Authorize(Roles = "Customer")]
[Route("customer/orders")]
public class CustomerOrderController : ApiController
{
    private readonly ISender _sender;

    public CustomerOrderController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command with { CustomerId = User.GetUserId() }, cancellationToken);

        return result.IsSuccess
            ? CreatedEnvelope(result.Value, "Order created successfully")
            : Problem(result.Errors);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetOrderById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetOrderByIdQuery(id, User.GetUserId());
        var result = await _sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? OkEnvelope(result.Value)
            : Problem(result.Errors);
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory(
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var query = new GetCustomerOrderHistoryQuery(User.GetUserId(), pageNumber, pageSize);

        var result = await _sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? OkEnvelope(result.Value)
            : Problem(result.Errors);
    }

    [HttpGet("track/{id:guid}")]
    public async Task<IActionResult> TrackOrder(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetOrderByIdQuery(id, User.GetUserId());
        var result = await _sender.Send(query, cancellationToken);

        return result.IsSuccess
            ? OkEnvelope(new { result.Value.Status })
            : Problem(result.Errors);
    }

    [HttpPut("cancel/{id:guid}")]
    public async Task<IActionResult> CancelOrder(
        Guid id, [FromBody] CancelOrderRequest? request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new CancelOrderCommand(id, User.GetUserId(), request?.Reason), cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : Problem(result.Errors);
    }
}

public sealed record CancelOrderRequest(string? Reason);
