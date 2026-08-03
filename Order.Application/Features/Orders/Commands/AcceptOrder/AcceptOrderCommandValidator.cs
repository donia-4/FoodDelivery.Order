using FluentValidation;

namespace Order.Application.Features.Orders.Commands.AcceptOrder;

public class AcceptOrderCommandValidator : AbstractValidator<AcceptOrderCommand>
{
    public AcceptOrderCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty();

        RuleFor(x => x.ChangedBy)
            .NotEmpty()
            .MaximumLength(200);
    }
}