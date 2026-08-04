using FluentValidation;

namespace Order.Application.Features.Carts.Commands.UpdateCartItemQuantity;

public sealed class UpdateCartItemQuantityCommandValidator : AbstractValidator<UpdateCartItemQuantityCommand>
{
    public UpdateCartItemQuantityCommandValidator()
    {
        RuleFor(c => c.CustomerId).NotEmpty();
        RuleFor(c => c.CartItemId).NotEmpty();
        RuleFor(c => c.Quantity).GreaterThan(0);
    }
}
