using DispatchManager.Application.Models.Common;
using DispatchManager.Application.Models.DTOs;
using MediatR;

namespace DispatchManager.Application.Features.Products.Queries.GetProducts;

public sealed record GetProductsQuery : IRequest<PagedResponse<ProductListDto>>
{
    public string? SearchTerm { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
