using MediatR;
using Order.Application.Common.Models;
using Order.Application.Features.Orders.Dtos.SearchOrders;
using Order.Domain.Results;

namespace Order.Application.Features.Orders.Queries.GetCustomerOrderHistory;

public sealed record GetCustomerOrderHistoryQuery(Guid CustomerId, int PageNumber = 1, int PageSize = 10)
    : IRequest<Result<PaginatedList<OrderSummaryDto>>>;
