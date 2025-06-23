using AutoMapper;
using DispatchManager.Application.Constants;
using DispatchManager.Application.Contracts.Infrastructure;
using DispatchManager.Application.Contracts.Persistence;
using DispatchManager.Application.Exceptions;
using DispatchManager.Application.Models.Common;
using DispatchManager.Application.Models.DTOs;
using DispatchManager.Domain.Entities;
using MediatR;

namespace DispatchManager.Application.Features.Orders.Queries.GetOrdersByCustomer;

public sealed class GetOrdersByCustomerQueryHandler : IRequestHandler<GetOrdersByCustomerQuery, PagedResponse<OrderListDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICacheService _cacheService;

    public GetOrdersByCustomerQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _cacheService = cacheService;
    }

    public async Task<PagedResponse<OrderListDto>> Handle(GetOrdersByCustomerQuery request, CancellationToken cancellationToken)
    {
        // Verificar que el cliente existe
        var customer = await _unitOfWork.Customers.GetByIdAsync(request.CustomerId, cancellationToken);
        if (customer == null)
        {
            throw new NotFoundException(nameof(Customer), request.CustomerId);
        }

        // Intentar obtener del cache
        var cacheKey = $"orders_customer_{request.CustomerId}_page_{request.PageNumber}_size_{request.PageSize}_sort_{request.SortBy}_{request.SortDescending}";
        var cachedResult = await _cacheService.GetAsync<PagedResponse<OrderListDto>>(cacheKey, cancellationToken);

        if (cachedResult != null)
        {
            return cachedResult;
        }

        // Obtener órdenes paginadas
        var orders = await _unitOfWork.Orders.GetOrdersByCustomerIdWithDetailsAsync(request.CustomerId, cancellationToken);

        // Aplicar ordenamiento
        var orderedOrders = request.SortDescending
            ? orders.OrderByDescending(o => GetSortProperty(o, request.SortBy))
            : orders.OrderBy(o => GetSortProperty(o, request.SortBy));

        // Aplicar paginación
        var totalCount = orderedOrders.Count();
        var pagedOrders = orderedOrders
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var orderDtos = _mapper.Map<IReadOnlyList<OrderListDto>>(pagedOrders);

        var response = new PagedResponse<OrderListDto>
        {
            Success = true,
            Message = "Orders retrieved successfully",
            Data = orderDtos,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };

        // Guardar en cache por 5 minutos
        await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(5), cancellationToken);

        return response;
    }

    private static object GetSortProperty(Order order, string sortBy)
    {
        return sortBy.ToLowerInvariant() switch
        {
            "createdat" => order.CreatedAt,
            "distance" => order.Distance.Kilometers,
            "cost" => order.Cost.Amount,
            "status" => order.Status,
            _ => order.CreatedAt
        };
    }
}
