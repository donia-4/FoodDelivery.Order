using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Order.Application.Features.Orders.Commands.UpdateOrderStatusByAdmin;
using Order.Application.Features.Orders.Queries.GetOrderStatistics;
using Order.Application.Features.Orders.Queries.SearchOrders;

namespace Order.API.Controllers;

[Authorize(Roles = "Admin")]
[Route("admin/orders")]
public class AdminOrdersController : ApiController
{
    private readonly ISender _sender;

    public AdminOrdersController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] SearchOrdersQuery query)
    {
        var result = await _sender.Send(query);

        return result.IsSuccess
            ? OkEnvelope(result.Value)
            : Problem(result.Errors);
    }

    [HttpGet("statistics")]
    public async Task<IActionResult> Statistics([FromQuery] GetOrderStatisticsQuery query)
    {
        var result = await _sender.Send(query);

        return result.IsSuccess
            ? OkEnvelope(result.Value)
            : Problem(result.Errors);
    }

    [HttpPost("update-status")]
    public async Task<IActionResult> UpdateStatus([FromBody] UpdateOrderStatusByAdminCommand command)
    {
        var result = await _sender.Send(command);

        return result.IsSuccess
            ? OkEnvelope(result.Value)
            : Problem(result.Errors);
    }
}