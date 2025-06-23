using DispatchManager.Application.Constants;
using FluentValidation;

namespace DispatchManager.Application.Features.Products.Commands.CreateProduct;

public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Nombre de producto es requerido")
            .MaximumLength(ApplicationConstants.Validation.MaxProductNameLength)
            .WithMessage($"Nombre de producto no puede exceder {ApplicationConstants.Validation.MaxProductNameLength} caracteres");

        RuleFor(x => x.UnitPrice)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Precio unitario no puede ser negativo");

        RuleFor(x => x.Unit)
            .NotEmpty()
            .WithMessage("Unidad es requerido")
            .MaximumLength(20)
            .WithMessage("Unidad no puede exceder 20 caracteres");

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage("Descripción no puede exceder 1000 caracteres");
    }
}