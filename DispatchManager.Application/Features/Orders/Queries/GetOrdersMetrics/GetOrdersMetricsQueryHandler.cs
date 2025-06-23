using DispatchManager.Application.Contracts.Infrastructure;
using DispatchManager.Application.Contracts.Persistence;
using DispatchManager.Application.Models.Common;
using DispatchManager.Application.Models.DTOs;
using DispatchManager.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DispatchManager.Application.Features.Orders.Queries.GetOrdersMetrics;

public sealed class GetOrdersMetricsQueryHandler : IRequestHandler<GetOrdersMetricsQuery, BaseResponse<OrdersMetricsDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;
    private readonly ILogger<GetOrdersMetricsQueryHandler> _logger;

    public GetOrdersMetricsQueryHandler(
        IUnitOfWork unitOfWork,
        ICacheService cacheService,
        ILogger<GetOrdersMetricsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<BaseResponse<OrdersMetricsDto>> Handle(GetOrdersMetricsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Generating orders metrics");

            // Cache key para las métricas
            var cacheKey = GenerateCacheKey(request);

            // Intentar obtener del cache
            var cachedMetrics = await _cacheService.GetAsync<OrdersMetricsDto>(cacheKey, cancellationToken);
            if (cachedMetrics != null)
            {
                _logger.LogInformation("Returning cached metrics");
                return new BaseResponse<OrdersMetricsDto>
                {
                    Success = true,
                    Message = "Métricas obtenidas exitosamente (cache)",
                    Data = cachedMetrics
                };
            }

            // Obtener todas las órdenes de la base de datos
            var allOrders = await _unitOfWork.Orders.GetAllAsync(cancellationToken);
            var allCustomers = await _unitOfWork.Customers.GetAllAsync(cancellationToken);

            // Fecha actual
            var now = DateTime.UtcNow;
            var today = now.Date;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
            var startOfMonth = new DateTime(today.Year, today.Month, 1);

            // Generar métricas
            var metrics = new OrdersMetricsDto
            {
                GeneratedAt = now,
                Period = request.StartDate.HasValue && request.EndDate.HasValue
                    ? $"{request.StartDate:yyyy-MM-dd} to {request.EndDate:yyyy-MM-dd}"
                    : "All Time"
            };

            // Filtrar órdenes por rango de fechas si se especifica
            var filteredOrders = allOrders;
            if (request.StartDate.HasValue)
                filteredOrders = (IReadOnlyList<Domain.Entities.Order>)filteredOrders.Where(o => o.CreatedAt >= request.StartDate.Value);
            if (request.EndDate.HasValue)
                filteredOrders = (IReadOnlyList<Domain.Entities.Order>)filteredOrders.Where(o => o.CreatedAt <= request.EndDate.Value);

            var orders = filteredOrders.ToList();

            // === MÉTRICAS DE ÓRDENES ===
            metrics.OrderCounts = CalculateOrderCountMetrics(orders, today, startOfWeek, startOfMonth);

            // === MÉTRICAS FINANCIERAS ===
            metrics.Financial = CalculateFinancialMetrics(orders, today, startOfWeek, startOfMonth);

            // === MÉTRICAS OPERATIVAS ===
            metrics.Operational = CalculateOperationalMetrics(orders);

            // === MÉTRICAS DE CLIENTES ===
            metrics.Customers = CalculateCustomerMetrics(allCustomers, orders, today, startOfWeek, startOfMonth);

            // === MÉTRICAS DE TENDENCIAS ===
            if (request.IncludeTrends)
            {
                metrics.Trends = CalculateTrendMetrics(allOrders.ToList(), allCustomers.ToList(), now);
            }

            // Guardar en cache por 5 minutos
            await _cacheService.SetAsync(cacheKey, metrics, TimeSpan.FromMinutes(5), cancellationToken);

            _logger.LogInformation("Metrics generated successfully. Total orders: {TotalOrders}", metrics.OrderCounts.TotalOrders);

            return new BaseResponse<OrdersMetricsDto>
            {
                Success = true,
                Message = "Métricas generadas exitosamente",
                Data = metrics
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating orders metrics");
            return new BaseResponse<OrdersMetricsDto>
            {
                Success = false,
                Message = "Error al generar las métricas de órdenes",
                Errors = new List<string> { ex.Message }
            };
        }
    }

    private static OrderCountMetrics CalculateOrderCountMetrics(
        List<Domain.Entities.Order> orders,
        DateTime today,
        DateTime startOfWeek,
        DateTime startOfMonth)
    {
        var totalOrders = orders.Count;
        var todayOrders = orders.Count(o => o.CreatedAt.Date == today);
        var weekOrders = orders.Count(o => o.CreatedAt >= startOfWeek);
        var monthOrders = orders.Count(o => o.CreatedAt >= startOfMonth);

        var pendingOrders = orders.Count(o => o.Status == OrderStatus.Sending);
        var inProgressOrders = orders.Count(o => o.Status == OrderStatus.InProgress);
        var deliveredOrders = orders.Count(o => o.Status == OrderStatus.Delivered);
        var cancelledOrders = orders.Count(o => o.Status == OrderStatus.Cancelled);

        var completedOrders = deliveredOrders + cancelledOrders;
        var deliverySuccessRate = completedOrders > 0 ? (decimal)deliveredOrders / completedOrders * 100 : 0;
        var cancellationRate = totalOrders > 0 ? (decimal)cancelledOrders / totalOrders * 100 : 0;

        return new OrderCountMetrics
        {
            TotalOrders = totalOrders,
            TodayOrders = todayOrders,
            WeekOrders = weekOrders,
            MonthOrders = monthOrders,
            PendingOrders = pendingOrders,
            InProgressOrders = inProgressOrders,
            DeliveredOrders = deliveredOrders,
            CancelledOrders = cancelledOrders,
            DeliverySuccessRate = Math.Round(deliverySuccessRate, 2),
            CancellationRate = Math.Round(cancellationRate, 2)
        };
    }

    private static FinancialMetrics CalculateFinancialMetrics(
        List<Domain.Entities.Order> orders,
        DateTime today,
        DateTime startOfWeek,
        DateTime startOfMonth)
    {
        var totalRevenue = orders.Sum(o => o.Cost.Amount);
        var todayRevenue = orders.Where(o => o.CreatedAt.Date == today).Sum(o => o.Cost.Amount);
        var weekRevenue = orders.Where(o => o.CreatedAt >= startOfWeek).Sum(o => o.Cost.Amount);
        var monthRevenue = orders.Where(o => o.CreatedAt >= startOfMonth).Sum(o => o.Cost.Amount);

        var averageOrderValue = orders.Count > 0 ? totalRevenue / orders.Count : 0;
        var averageDeliveryCost = orders.Count > 0 ? orders.Average(o => o.Cost.Amount) : 0;

        return new FinancialMetrics
        {
            TotalRevenue = Math.Round(totalRevenue, 2),
            TodayRevenue = Math.Round(todayRevenue, 2),
            WeekRevenue = Math.Round(weekRevenue, 2),
            MonthRevenue = Math.Round(monthRevenue, 2),
            AverageOrderValue = Math.Round(averageOrderValue, 2),
            AverageDeliveryCost = Math.Round(averageDeliveryCost, 2),
            Currency = orders.FirstOrDefault()?.Cost.Currency ?? "USD"
        };
    }

    private static OperationalMetrics CalculateOperationalMetrics(List<Domain.Entities.Order> orders)
    {
        if (!orders.Any())
        {
            return new OperationalMetrics();
        }

        var averageDistance = orders.Average(o => o.Distance.Kilometers);
        var totalDistance = orders.Sum(o => o.Distance.Kilometers);

        var shortDistanceOrders = orders.Count(o => o.Distance.Kilometers < 5);
        var mediumDistanceOrders = orders.Count(o => o.Distance.Kilometers >= 5 && o.Distance.Kilometers <= 15);
        var longDistanceOrders = orders.Count(o => o.Distance.Kilometers > 15);

        var daysWithOrders = orders.GroupBy(o => o.CreatedAt.Date).Count();
        var averageOrdersPerDay = daysWithOrders > 0 ? (decimal)orders.Count / daysWithOrders : 0;

        return new OperationalMetrics
        {
            AverageDistance = (decimal)Math.Round(averageDistance, 2),
            TotalDistance = (decimal)Math.Round(totalDistance, 2),
            ShortDistanceOrders = shortDistanceOrders,
            MediumDistanceOrders = mediumDistanceOrders,
            LongDistanceOrders = longDistanceOrders,
            AverageOrdersPerDay = Math.Round(averageOrdersPerDay, 2)
        };
    }

    private static CustomerMetrics CalculateCustomerMetrics(
        IEnumerable<Domain.Entities.Customer> allCustomers,
        List<Domain.Entities.Order> orders,
        DateTime today,
        DateTime startOfWeek,
        DateTime startOfMonth)
    {
        var customers = allCustomers.ToList();
        var totalCustomers = customers.Count;

        var newCustomersToday = customers.Count(c => c.CreatedAt.Date == today);
        var newCustomersWeek = customers.Count(c => c.CreatedAt >= startOfWeek);
        var newCustomersMonth = customers.Count(c => c.CreatedAt >= startOfMonth);

        var last30Days = today.AddDays(-30);
        var activeCustomers = orders
            .Where(o => o.CreatedAt >= last30Days)
            .Select(o => o.CustomerId)
            .Distinct()
            .Count();

        var averageOrdersPerCustomer = totalCustomers > 0 ? (decimal)orders.Count / totalCustomers : 0;

        return new CustomerMetrics
        {
            TotalCustomers = totalCustomers,
            NewCustomersToday = newCustomersToday,
            NewCustomersWeek = newCustomersWeek,
            NewCustomersMonth = newCustomersMonth,
            ActiveCustomers = activeCustomers,
            AverageOrdersPerCustomer = Math.Round(averageOrdersPerCustomer, 2)
        };
    }

    private static TrendMetrics CalculateTrendMetrics(
        List<Domain.Entities.Order> allOrders,
        List<Domain.Entities.Customer> allCustomers,
        DateTime now)
    {
        var trends = new TrendMetrics();

        // Calcular crecimiento mensual
        var thisMonth = new DateTime(now.Year, now.Month, 1);
        var lastMonth = thisMonth.AddMonths(-1);

        var thisMonthOrders = allOrders.Count(o => o.CreatedAt >= thisMonth);
        var lastMonthOrders = allOrders.Count(o => o.CreatedAt >= lastMonth && o.CreatedAt < thisMonth);

        var thisMonthRevenue = allOrders.Where(o => o.CreatedAt >= thisMonth).Sum(o => o.Cost.Amount);
        var lastMonthRevenue = allOrders.Where(o => o.CreatedAt >= lastMonth && o.CreatedAt < thisMonth).Sum(o => o.Cost.Amount);

        var thisMonthCustomers = allCustomers.Count(c => c.CreatedAt >= thisMonth);
        var lastMonthCustomers = allCustomers.Count(c => c.CreatedAt >= lastMonth && c.CreatedAt < thisMonth);

        // Calcular tasas de crecimiento
        trends.OrderGrowthRate = CalculateGrowthRate(thisMonthOrders, lastMonthOrders);
        trends.RevenueGrowthRate = CalculateGrowthRate(thisMonthRevenue, lastMonthRevenue);
        trends.CustomerGrowthRate = CalculateGrowthRate(thisMonthCustomers, lastMonthCustomers);

        // Últimos 7 días
        for (int i = 6; i >= 0; i--)
        {
            var date = now.Date.AddDays(-i);
            var dayOrders = allOrders.Where(o => o.CreatedAt.Date == date);

            trends.Last7Days.Add(new DailyMetric
            {
                Date = date,
                OrderCount = dayOrders.Count(),
                Revenue = Math.Round(dayOrders.Sum(o => o.Cost.Amount), 2)
            });
        }

        // Últimos 6 meses
        for (int i = 5; i >= 0; i--)
        {
            var monthStart = now.AddMonths(-i).Date;
            monthStart = new DateTime(monthStart.Year, monthStart.Month, 1);
            var monthEnd = monthStart.AddMonths(1);

            var monthOrders = allOrders.Where(o => o.CreatedAt >= monthStart && o.CreatedAt < monthEnd);
            var monthCustomers = allCustomers.Where(c => c.CreatedAt >= monthStart && c.CreatedAt < monthEnd);

            trends.Last6Months.Add(new MonthlyMetric
            {
                Year = monthStart.Year,
                Month = monthStart.Month,
                MonthName = monthStart.ToString("MMM yyyy"),
                OrderCount = monthOrders.Count(),
                Revenue = Math.Round(monthOrders.Sum(o => o.Cost.Amount), 2),
                NewCustomers = monthCustomers.Count()
            });
        }

        return trends;
    }

    private static decimal CalculateGrowthRate(decimal current, decimal previous)
    {
        if (previous == 0) return current > 0 ? 100 : 0;
        return Math.Round(((current - previous) / previous) * 100, 2);
    }

    private static decimal CalculateGrowthRate(int current, int previous)
    {
        return CalculateGrowthRate((decimal)current, (decimal)previous);
    }

    private static string GenerateCacheKey(GetOrdersMetricsQuery request)
    {
        var keyParts = new List<string> { "orders_metrics" };

        if (request.StartDate.HasValue)
            keyParts.Add($"start_{request.StartDate:yyyyMMdd}");

        if (request.EndDate.HasValue)
            keyParts.Add($"end_{request.EndDate:yyyyMMdd}");

        keyParts.Add($"trends_{request.IncludeTrends}");

        return string.Join("_", keyParts);
    }
}
