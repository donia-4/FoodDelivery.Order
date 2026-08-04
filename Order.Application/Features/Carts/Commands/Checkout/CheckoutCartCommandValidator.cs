using FluentValidation;

namespace Order.Application.Features.Carts.Commands.Checkout;

public sealed class CheckoutCartCommandValidator : AbstractValidator<CheckoutCartCommand>
{
    public CheckoutCartCommandValidator()
    {
        RuleFor(c => c.CustomerId).NotEmpty();
        RuleFor(c => c.AddressId).NotEmpty();
        RuleFor(c => c.DeliveryFee).GreaterThanOrEqualTo(0);
    }
}
