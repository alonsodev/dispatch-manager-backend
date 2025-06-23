using AutoMapper;
using DispatchManager.Application.Contracts.Infrastructure;
using DispatchManager.Application.Contracts.Persistence;
using DispatchManager.Application.Models.Common;
using DispatchManager.Application.Models.DTOs;
using MediatR;

namespace DispatchManager.Application.Features.Products.Queries.GetProducts;

public sealed class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, PagedResponse<ProductListDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICacheService _cacheService;

    public GetProductsQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _cacheService = cacheService;
    }

    public async Task<PagedResponse<ProductListDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"products_search_{request.SearchTerm}_price_{request.MinPrice}_{request.MaxPrice}_page_{request.PageNumber}_size_{request.PageSize}";
        var cachedResult = await _cacheService.GetAsync<PagedResponse<ProductListDto>>(cacheKey, cancellationToken);

        if (cachedResult != null)
        {
            return cachedResult;
        }

        var products = await GetFilteredProductsAsync(request, cancellationToken);

        var totalCount = products.Count;
        var pagedProducts = products
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var productDtos = _mapper.Map<IReadOnlyList<ProductListDto>>(pagedProducts);

        var response = new PagedResponse<ProductListDto>
        {
            Success = true,
            Message = "Productos recuperados con éxito",
            Data = productDtos,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };

        await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(15), cancellationToken);
        return response;
    }

    private async Task<IReadOnlyList<Domain.Entities.Product>> GetFilteredProductsAsync(
        GetProductsQuery request,
        CancellationToken cancellationToken)
    {
        // Aplicar filtros según los parámetros
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            return await _unitOfWork.Products.SearchByNameAsync(request.SearchTerm, cancellationToken);
        }

        if (request.MinPrice.HasValue && request.MaxPrice.HasValue)
        {
            return await _unitOfWork.Products.GetProductsByPriceRangeAsync(
                request.MinPrice.Value,
                request.MaxPrice.Value,
                cancellationToken);
        }

        return await _unitOfWork.Products.GetActiveProductsAsync(cancellationToken);
    }
}