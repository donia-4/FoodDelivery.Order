using Azure.Core;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Order.Application.Features.Orders.Commands.UpdateOrderStatusByAdmin;
using Order.Application.Features.Orders.Queries.GetOrderStatistics;
using Order.Application.Features.Orders.Queries.SearchOrders;
using Order.Domain.Orders.Enums;

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
    public async Task<IActionResult> Search(
        [FromQuery] Guid? customerId,
        [FromQuery] Guid? restaurantId,
        [FromQuery] string? orderNumber,
        [FromQuery] OrderStatus? status,
        [FromQuery] DateTimeOffset? fromDate,
        [FromQuery] DateTimeOffset? toDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new SearchOrdersQuery(
            customerId,
            restaurantId,
            orderNumber,
            status,
            fromDate,
            toDate,
            page,
            pageSize);

        var result = await _sender.Send(query);

        return result.IsSuccess
            ? OkEnvelope(result.Value)
            : Problem(result.Errors);
    }

    [HttpGet("statistics")]
    public async Task<IActionResult> Statistics([FromQuery] DateTimeOffset? FromDate, DateTimeOffset? ToDate)
    {
        var query = new GetOrderStatisticsQuery(FromDate,ToDate);

        var result = await _sender.Send(query);

        return result.IsSuccess
            ? OkEnvelope(result.Value)
            : Problem(result.Errors);
    }

    [HttpPost("update-status")]
    public async Task<IActionResult> UpdateStatus(
    [FromForm] UpdateOrderStatusByAdminCommand command)
    {
        var result = await _sender.Send(command);

        return result.IsSuccess
            ? OkEnvelope(result.Value)
            : Problem(result.Errors);
    }
}