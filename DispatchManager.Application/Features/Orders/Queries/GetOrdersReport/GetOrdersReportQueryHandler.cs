using DispatchManager.Application.Contracts.Infrastructure;
using DispatchManager.Application.Contracts.Persistence;
using DispatchManager.Application.Models.Common;
using DispatchManager.Application.Models.DTOs;
using MediatR;

namespace DispatchManager.Application.Features.Orders.Queries.GetOrdersReport;

public sealed class GetOrdersReportQueryHandler : IRequestHandler<GetOrdersReportQuery, BaseResponse<OrdersReportDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;

    public GetOrdersReportQueryHandler(
        IUnitOfWork unitOfWork,
        ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
    }

    public async Task<BaseResponse<OrdersReportDto>> Handle(GetOrdersReportQuery request, CancellationToken cancellationToken)
    {
        // Crear cache key
        var cacheKey = $"orders_report_{request.CustomerId}_{request.StartDate:yyyyMMdd}_{request.EndDate:yyyyMMdd}_{request.IncludeGlobalSummary}";

        // Intentar obtener del cache
        var cachedReport = await _cacheService.GetAsync<OrdersReportDto>(cacheKey, cancellationToken);
        if (cachedReport != null)
        {
            return new BaseResponse<OrdersReportDto>
            {
                Success = true,
                Message = "Report retrieved successfully",
                Data = cachedReport
            };
        }

        var report = new OrdersReportDto
        {
            GeneratedAt = DateTime.UtcNow
        };

        if (request.CustomerId.HasValue)
        {
            // Reporte específico de cliente
            var customerReport = await GenerateCustomerReportAsync(request.CustomerId.Value, request.StartDate, request.EndDate, cancellationToken);
            if (customerReport != null)
            {
                report.CustomerReports.Add(customerReport);
                report.TotalOrders = customerReport.TotalOrders;
            }
        }
        else
        {
            // Reporte de todos los clientes
            var customerReports = await _unitOfWork.Orders.GetOrderCountByCustomerAndIntervalAsync(cancellationToken);

            var groupedReports = customerReports
                .GroupBy(x => new { x.CustomerId, x.CustomerName })
                .Select(g => new CustomerOrdersReportDto
                {
                    CustomerId = g.Key.CustomerId,
                    CustomerName = g.Key.CustomerName,
                    IntervalCounts = g.ToDictionary(x => x.DistanceInterval, x => x.OrderCount),
                    TotalOrders = g.Sum(x => x.OrderCount)
                })
                .ToList();

            report.CustomerReports = groupedReports;
            report.TotalOrders = groupedReports.Sum(x => x.TotalOrders);
        }

        if (request.IncludeGlobalSummary)
        {
            report.GlobalIntervalCounts = await _unitOfWork.Orders.GetOrderCountByDistanceIntervalAsync(cancellationToken);
        }

        // Guardar en cache por 10 minutos
        await _cacheService.SetAsync(cacheKey, report, TimeSpan.FromMinutes(10), cancellationToken);

        return new BaseResponse<OrdersReportDto>
        {
            Success = true,
            Message = "Report generated successfully",
            Data = report
        };
    }

    private async Task<CustomerOrdersReportDto?> GenerateCustomerReportAsync(
        Guid customerId,
        DateTime? startDate,
        DateTime? endDate,
        CancellationToken cancellationToken)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(customerId, cancellationToken);
        if (customer == null)
            return null;

        var intervalCounts = await _unitOfWork.Orders.GetOrderCountByDistanceIntervalForCustomerAsync(customerId, cancellationToken);

        return new CustomerOrdersReportDto
        {
            CustomerId = customerId,
            CustomerName = customer.Name,
            IntervalCounts = intervalCounts,
            TotalOrders = intervalCounts.Values.Sum()
        };
    }
}