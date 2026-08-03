using FluentValidation;

namespace Order.Application.Features.Orders.Commands.RejectOrder;

public class RejectOrderCommandValidator : AbstractValidator<RejectOrderCommand>
{
    public RejectOrderCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty();

        RuleFor(x => x.ChangedBy)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Reason)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrEmpty(x.Reason));
    }
}