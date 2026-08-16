namespace Order.Application.Features.Carts.Dtos;

public sealed record CartItemDto(
    Guid Id,
    Guid MenuItemId,
    int Quantity,
    decimal UnitPrice,
    decimal Total,
    string? Notes);

public sealed record CartDto(
    Guid Id,
    Guid CustomerId,
    Guid RestaurantId,
    IReadOnlyCollection<CartItemDto> Items,
    decimal SubTotal);

public static class CartMappingExtensions
{
    public static CartDto ToDto(this Order.Domain.Carts.Cart cart) =>
        new(
            cart.Id,
            cart.CustomerId,
            cart.RestaurantId,
            cart.Items.Select(i => new CartItemDto(
                i.Id, i.MenuItemId, i.Quantity, i.UnitPrice, i.Total, i.Notes)).ToList(),
            cart.Items.Sum(i => i.Total));
}
