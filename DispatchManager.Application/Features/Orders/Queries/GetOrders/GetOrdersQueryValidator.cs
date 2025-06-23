using DispatchManager.Application.Constants;
using FluentValidation;

namespace DispatchManager.Application.Features.Orders.Queries.GetOrders;

public sealed class GetOrdersQueryValidator : AbstractValidator<GetOrdersQuery>
{
    private static readonly string[] ValidSortFields = {
        "createdat", "customername", "productname", "status", "distance", "cost"
    };

    private static readonly string[] ValidSortDirections = { "asc", "desc" };

    public GetOrdersQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("Número de página debe ser mayor a 0");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, ApplicationConstants.Orders.MaxOrdersPerQuery)
            .WithMessage($"Tamaño de página debe estar entre 1 y {ApplicationConstants.Orders.MaxOrdersPerQuery}");

        RuleFor(x => x.SortBy)
            .Must(BeValidSortField)
            .WithMessage($"Campo de ordenamiento inválido. Valores permitidos: {string.Join(", ", ValidSortFields)}");

        RuleFor(x => x.SortDirection)
            .Must(BeValidSortDirection)
            .WithMessage($"Dirección de ordenamiento inválida. Valores permitidos: {string.Join(", ", ValidSortDirections)}");

        RuleFor(x => x.StartDate)
            .LessThanOrEqualTo(x => x.EndDate)
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
            .WithMessage("Fecha de inicio debe ser menor o igual a fecha de fin");

        RuleFor(x => x.EndDate)
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1))
            .When(x => x.EndDate.HasValue)
            .WithMessage("Fecha de fin no puede ser mayor a mañana");

        RuleFor(x => x.SearchTerm)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.SearchTerm))
            .WithMessage("Término de búsqueda no puede exceder 100 caracteres");
    }

    private static bool BeValidSortField(string sortBy)
    {
        return ValidSortFields.Contains(sortBy.ToLowerInvariant());
    }

    private static bool BeValidSortDirection(string sortDirection)
    {
        return ValidSortDirections.Contains(sortDirection.ToLowerInvariant());
    }
}