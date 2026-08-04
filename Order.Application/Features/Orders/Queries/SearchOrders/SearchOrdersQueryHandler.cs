using MediatR;
using Microsoft.Extensions.Logging;
using Order.Application.Common.Interfaces.Repositories;
using Order.Application.Common.Models;
using Order.Application.Features.Orders.Dtos.SearchOrders;
using Order.Domain.Results;

namespace Order.Application.Features.Orders.Queries.SearchOrders;

public sealed class SearchOrdersQueryHandler(IOrderRepository orderRepository, ILogger<SearchOrdersQueryHandler> logger)
    : IRequestHandler<SearchOrdersQuery, Result<PaginatedList<OrderSummaryDto>>>
{
    public async Task<Result<PaginatedList<OrderSummaryDto>>> Handle(
        SearchOrdersQuery request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Starting search orders query. CustomerId: {CustomerId}, RestaurantId: {RestaurantId}, " +
            "OrderNumber: {OrderNumber}, Status: {Status}, Page: {Page}, PageSize: {PageSize}",
            request.CustomerId,
            request.RestaurantId,
            request.OrderNumber,
            request.Status,
            request.Page,
            request.PageSize);

        var paginated = await orderRepository.SearchAsync(
            request.CustomerId,
            request.RestaurantId,
            request.OrderNumber,
            request.Status,
            request.FromDate,
            request.ToDate,
            request.Page,
            request.PageSize,
            cancellationToken);

        logger.LogInformation(
            "Search orders query completed successfully. TotalCount: {TotalCount}, TotalPages: {TotalPages}, " +
            "ReturnedItems: {ReturnedItems}",
            paginated.TotalCount,
            paginated.TotalPages,
            paginated.Items.Count);

        return new PaginatedList<OrderSummaryDto>
        {
            PageNumber = paginated.PageNumber,
            PageSize = paginated.PageSize,
            TotalCount = paginated.TotalCount,
            TotalPages = paginated.TotalPages,
            Items = paginated.Items.Select(o => new OrderSummaryDto(
                o.Id,
                o.OrderNumber,
                o.CustomerId,
                o.RestaurantId,
                o.Status,
                o.Total,
                o.CreatedAtUtc
            )).ToList()
        };
    }
}