using AutoMapper;
using DispatchManager.Application.Contracts.Infrastructure;
using DispatchManager.Application.Contracts.Persistence;
using DispatchManager.Application.Models.Common;
using DispatchManager.Application.Models.DTOs;
using MediatR;

namespace DispatchManager.Application.Features.Customers.Queries.GetCustomers;

public sealed class GetCustomersQueryHandler : IRequestHandler<GetCustomersQuery, PagedResponse<CustomerListDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICacheService _cacheService;

    public GetCustomersQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _cacheService = cacheService;
    }

    public async Task<PagedResponse<CustomerListDto>> Handle(GetCustomersQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"customers_search_{request.SearchTerm}_page_{request.PageNumber}_size_{request.PageSize}";
        var cachedResult = await _cacheService.GetAsync<PagedResponse<CustomerListDto>>(cacheKey, cancellationToken);

        if (cachedResult != null)
        {
            return cachedResult;
        }

        var customers = string.IsNullOrWhiteSpace(request.SearchTerm)
            ? await _unitOfWork.Customers.GetAllAsync(cancellationToken)
            : await _unitOfWork.Customers.SearchByNameAsync(request.SearchTerm, cancellationToken);

        var totalCount = customers.Count;
        var pagedCustomers = customers
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var customerDtos = _mapper.Map<IReadOnlyList<CustomerListDto>>(pagedCustomers);

        var response = new PagedResponse<CustomerListDto>
        {
            Success = true,
            Message = "Clientes recuperados con éxito",
            Data = customerDtos,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };

        await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(15), cancellationToken);
        return response;
    }
}