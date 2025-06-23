using DispatchManager.Application.Constants;
using FluentValidation;

namespace DispatchManager.Application.Features.Customers.Commands.CreateCustomer;

public sealed class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Nombre de Cliente es requerido")
            .MaximumLength(ApplicationConstants.Validation.MaxCustomerNameLength)
            .WithMessage($"Nombre de Cliente no puede exceder {ApplicationConstants.Validation.MaxCustomerNameLength} caracteres");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Correo es requerido")
            .EmailAddress()
            .WithMessage("Formato de email inválido")
            .MaximumLength(ApplicationConstants.Validation.MaxEmailLength)
            .WithMessage($"Correo no puede exceder {ApplicationConstants.Validation.MaxEmailLength} caracteres");

        RuleFor(x => x.Phone)
            .NotEmpty()
            .WithMessage("Teléfono es requerido")
            .MaximumLength(ApplicationConstants.Validation.MaxPhoneLength)
            .WithMessage($"Teléfono no puede exceder {ApplicationConstants.Validation.MaxPhoneLength} caracteres");
    }
}