using FluentValidation;

namespace DispatchManager.Application.Features.Orders.Queries.GetOrderDetail;

public sealed class GetOrderDetailQueryValidator : AbstractValidator<GetOrderDetailQuery>
{
    public GetOrderDetailQueryValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("Id de Orden es requerido");
    }
}