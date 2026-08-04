using MediatR;
using Order.Application.Common.Interfaces.Repositories;
using Order.Application.Common.Models;
using Order.Application.Features.Orders.Dtos.SearchOrders;
using Order.Domain.Results;

namespace Order.Application.Features.Orders.Queries.GetCustomerOrderHistory;

public sealed class GetCustomerOrderHistoryQueryHandler(IOrderRepository orderRepository)
    : IRequestHandler<GetCustomerOrderHistoryQuery, Result<PaginatedList<OrderSummaryDto>>>
{
    public async Task<Result<PaginatedList<OrderSummaryDto>>> Handle(
        GetCustomerOrderHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var paginated = await orderRepository.SearchAsync(
            customerId: request.CustomerId,
            restaurantId: null,
            orderNumber: null,
            status: null,
            fromDate: null,
            toDate: null,
            page: request.PageNumber,
            pageSize: request.PageSize,
            ct: cancellationToken);

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
