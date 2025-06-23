using DispatchManager.Application.Models.Common;
using DispatchManager.Application.Models.DTOs;
using MediatR;

namespace DispatchManager.Application.Features.Products.Commands.CreateProduct;

public sealed record CreateProductCommand : IRequest<BaseResponse<ProductDto>>
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal UnitPrice { get; init; }
    public string Unit { get; init; } = string.Empty;
}