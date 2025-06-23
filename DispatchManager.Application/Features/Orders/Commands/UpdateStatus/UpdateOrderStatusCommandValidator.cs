using DispatchManager.Domain.Enums;
using FluentValidation;

namespace DispatchManager.Application.Features.Orders.Commands.UpdateOrderStatus;

public sealed class UpdateOrderStatusCommandValidator : AbstractValidator<UpdateOrderStatusCommand>
{
    public UpdateOrderStatusCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("ID de Orden es requerido");

        RuleFor(x => x.NewStatus)
            .IsInEnum()
            .WithMessage("Estado de Orden inválido");
    }
}