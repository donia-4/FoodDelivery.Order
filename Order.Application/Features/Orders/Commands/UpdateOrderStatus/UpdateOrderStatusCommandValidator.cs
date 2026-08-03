using FluentValidation;
using Order.Domain.Orders.Enums;

namespace Order.Application.Features.Orders.Commands.UpdateOrderStatus;

public class UpdateOrderStatusCommandValidator : AbstractValidator<UpdateOrderStatusCommand>
{
    public UpdateOrderStatusCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty();

        RuleFor(x => x.ChangedBy)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.NewStatus)
            .Must(status => status is OrderStatus.Preparing or OrderStatus.Ready)
            .WithMessage("Restaurant owners can only transition to Preparing or Ready.");
    }
}