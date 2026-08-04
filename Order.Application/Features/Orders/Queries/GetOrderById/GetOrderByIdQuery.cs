using MediatR;
using Order.Application.Features.Orders.Dtos.GetOrderDetails;
using Order.Domain.Results;

namespace Order.Application.Features.Orders.Queries.GetOrderById;

// Deliberately NOT an ICachedQuery: the ownership check below runs inside the
// handler, so caching this would risk serving one customer's cached order to
// another customer whose request short-circuits at the caching pipeline behavior.
public sealed record GetOrderByIdQuery(Guid Id, Guid CustomerId)
    : IRequest<Result<OrderDetailsDto>>;
