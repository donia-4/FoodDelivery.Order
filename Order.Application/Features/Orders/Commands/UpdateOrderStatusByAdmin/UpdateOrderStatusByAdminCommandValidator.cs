using FluentValidation;
using Order.Domain.Orders.Enums;

namespace Order.Application.Features.Orders.Commands.UpdateOrderStatusByAdmin;

public class UpdateOrderStatusByAdminCommandValidator : AbstractValidator<UpdateOrderStatusByAdminCommand>
{
    public UpdateOrderStatusByAdminCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty();


        RuleFor(x => x.NewStatus)
            .IsInEnum()
            .Must(status => status != OrderStatus.Pending)
            .WithMessage("Cannot transition to Pending status.");
    }
}