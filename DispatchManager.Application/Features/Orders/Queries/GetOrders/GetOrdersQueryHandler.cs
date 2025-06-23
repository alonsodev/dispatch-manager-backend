using AutoMapper;
using DispatchManager.Application.Constants;
using DispatchManager.Application.Contracts.Infrastructure;
using DispatchManager.Application.Contracts.Persistence;
using DispatchManager.Application.Models.Common;
using DispatchManager.Application.Models.DTOs;
using DispatchManager.Domain.Entities;
using MediatR;

namespace DispatchManager.Application.Features.Orders.Queries.GetOrders;

public sealed class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, PagedResponse<OrderListDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICacheService _cacheService;

    public GetOrdersQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _cacheService = cacheService;
    }

    public async Task<PagedResponse<OrderListDto>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        // Generar clave de cache única
        var cacheKey = $"orders_page_{request.PageNumber}_size_{request.PageSize}_sort_{request.SortBy}_{request.SortDirection}_search_{request.SearchTerm}_status_{request.Status}_customer_{request.CustomerId}_start_{request.StartDate:yyyyMMdd}_end_{request.EndDate:yyyyMMdd}";

        var cachedResult = await _cacheService.GetAsync<PagedResponse<OrderListDto>>(cacheKey, cancellationToken);
        if (cachedResult != null)
        {
            return cachedResult;
        }

        // Obtener órdenes con detalles (customer, product)
        var orders = await _unitOfWork.Orders.GetOrdersWithDetailsAsync(cancellationToken);

        // Aplicar filtros
        var filteredOrders = ApplyFilters(orders, request);

        // Aplicar ordenamiento
        var orderedOrders = ApplySorting(filteredOrders, request);

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
            Message = "Órdenes recuperadas con éxito",
            Data = orderDtos,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };

        // Guardar en cache por 5 minutos
        await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(5), cancellationToken);

        return response;
    }

    private static IEnumerable<Order> ApplyFilters(IEnumerable<Order> orders, GetOrdersQuery request)
    {
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLowerInvariant();
            orders = orders.Where(o =>
                o.Customer.Name.ToLowerInvariant().Contains(searchTerm) ||
                o.Product.Name.ToLowerInvariant().Contains(searchTerm) ||
                o.Id.ToString().Contains(searchTerm));
        }

        if (request.Status.HasValue)
        {
            orders = orders.Where(o => o.Status == request.Status.Value);
        }

        if (request.CustomerId.HasValue)
        {
            orders = orders.Where(o => o.CustomerId == request.CustomerId.Value);
        }

        if (request.StartDate.HasValue)
        {
            orders = orders.Where(o => o.CreatedAt >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            orders = orders.Where(o => o.CreatedAt <= request.EndDate.Value.AddDays(1));
        }

        return orders;
    }

    private static IEnumerable<Order> ApplySorting(IEnumerable<Order> orders, GetOrdersQuery request)
    {
        return request.SortBy.ToLowerInvariant() switch
        {
            "createdat" => request.SortDescending
                ? orders.OrderByDescending(o => o.CreatedAt)
                : orders.OrderBy(o => o.CreatedAt),
            "customername" => request.SortDescending
                ? orders.OrderByDescending(o => o.Customer.Name)
                : orders.OrderBy(o => o.Customer.Name),
            "productname" => request.SortDescending
                ? orders.OrderByDescending(o => o.Product.Name)
                : orders.OrderBy(o => o.Product.Name),
            "status" => request.SortDescending
                ? orders.OrderByDescending(o => o.Status)
                : orders.OrderBy(o => o.Status),
            "distance" => request.SortDescending
                ? orders.OrderByDescending(o => o.Distance.Kilometers)
                : orders.OrderBy(o => o.Distance.Kilometers),
            "cost" => request.SortDescending
                ? orders.OrderByDescending(o => o.Cost.Amount)
                : orders.OrderBy(o => o.Cost.Amount),
            _ => request.SortDescending
                ? orders.OrderByDescending(o => o.CreatedAt)
                : orders.OrderBy(o => o.CreatedAt)
        };
    }
}