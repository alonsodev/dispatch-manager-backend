using DispatchManager.Application.Constants;
using FluentValidation;

namespace DispatchManager.Application.Features.Orders.Queries.GetOrdersByCustomer;

public sealed class GetOrdersByCustomerQueryValidator : AbstractValidator<GetOrdersByCustomerQuery>
{
    public GetOrdersByCustomerQueryValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("Id de Cliente es requerido");

        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("Número de página debe ser mayor a 0");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, ApplicationConstants.Orders.MaxOrdersPerQuery)
            .WithMessage($"Tamaño de ágina debe estar entre 1 y {ApplicationConstants.Orders.MaxOrdersPerQuery}");

        RuleFor(x => x.SortBy)
            .Must(BeValidSortField)
            .WithMessage("Campo de ordenamiento inválido. Los campos válidos son: CreatedAt, Distance, Cost, Status");
    }

    private static bool BeValidSortField(string sortBy)
    {
        var validFields = new[] { "CreatedAt", "Distance", "Cost", "Status" };
        return validFields.Contains(sortBy, StringComparer.OrdinalIgnoreCase);
    }
}