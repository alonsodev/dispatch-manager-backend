using DispatchManager.Application.Constants;
using FluentValidation;

namespace DispatchManager.Application.Features.Orders.Commands.CreateOrder;

public sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("Id de Cliente es requerido");

        RuleFor(x => x.ProductId)
            .NotEmpty()
            .WithMessage("Id de Producto es requerido");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Cantidad debe ser mayor a cero");

        RuleFor(x => x.Origin)
            .NotNull()
            .WithMessage("Coordenadas de Origen son requeridos");

        RuleFor(x => x.Origin.Latitude)
            .InclusiveBetween(-90, 90)
            .When(x => x.Origin != null)
            .WithMessage("Latitud de Origen debe estar entre -90 y 90 grados");

        RuleFor(x => x.Origin.Longitude)
            .InclusiveBetween(-180, 180)
            .When(x => x.Origin != null)
            .WithMessage("Longitud de Origen debe estar entre -180 y 180 grados");

        RuleFor(x => x.Destination)
            .NotNull()
            .WithMessage("Coordenadas de Destino son requeridos");

        RuleFor(x => x.Destination.Latitude)
            .InclusiveBetween(-90, 90)
            .When(x => x.Destination != null)
            .WithMessage("Latitud de Destino debe estar entre -90 y 90 grados");

        RuleFor(x => x.Destination.Longitude)
            .InclusiveBetween(-180, 180)
            .When(x => x.Destination != null)
            .WithMessage("Longitud de Destino debe estar entre -180 y 180 grados");

        RuleFor(x => x)
            .Must(HaveDifferentOriginAndDestination)
            .WithMessage("Coordenadas de origen y destino no pueden ser el mismo");
    }

    private static bool HaveDifferentOriginAndDestination(CreateOrderCommand command)
    {
        if (command.Origin == null || command.Destination == null)
            return true; 

        return Math.Abs(command.Origin.Latitude - command.Destination.Latitude) > 0.0001 ||
               Math.Abs(command.Origin.Longitude - command.Destination.Longitude) > 0.0001;
    }
}