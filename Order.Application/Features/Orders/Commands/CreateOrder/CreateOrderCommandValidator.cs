using FluentValidation;

namespace Order.Application.Features.Orders.Commands.CreateOrder;

public sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(c => c.CustomerId).NotEmpty();
        RuleFor(c => c.RestaurantId).NotEmpty();
        RuleFor(c => c.AddressId).NotEmpty();
        RuleFor(c => c.DeliveryFee).GreaterThanOrEqualTo(0);
        RuleFor(c => c.Items).NotEmpty().WithMessage("An order must contain at least one item.");

        RuleForEach(c => c.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.MenuItemId).NotEmpty();
            item.RuleFor(i => i.Quantity).GreaterThan(0);
        });
    }
}
