namespace DispatchManager.Application.Models.DTOs;

public sealed class OrdersMetricsDto
{
    // Métricas Generales de Órdenes
    public OrderCountMetrics OrderCounts { get; set; } = new();

    // Métricas Financieras
    public FinancialMetrics Financial { get; set; } = new();

    // Métricas Operativas
    public OperationalMetrics Operational { get; set; } = new();

    // Métricas de Clientes
    public CustomerMetrics Customers { get; set; } = new();

    // Métricas de Tendencias
    public TrendMetrics Trends { get; set; } = new();

    // Metadata
    public DateTime GeneratedAt { get; set; }
    public string Period { get; set; } = "All Time";
}

public sealed class OrderCountMetrics
{
    public int TotalOrders { get; set; }
    public int TodayOrders { get; set; }
    public int WeekOrders { get; set; }
    public int MonthOrders { get; set; }

    // Por Estado
    public int PendingOrders { get; set; }
    public int InProgressOrders { get; set; }
    public int DeliveredOrders { get; set; }
    public int CancelledOrders { get; set; }

    // Porcentajes
    public decimal DeliverySuccessRate { get; set; }
    public decimal CancellationRate { get; set; }
}

public sealed class FinancialMetrics
{
    public decimal TotalRevenue { get; set; }
    public decimal TodayRevenue { get; set; }
    public decimal WeekRevenue { get; set; }
    public decimal MonthRevenue { get; set; }

    public decimal AverageOrderValue { get; set; }
    public decimal AverageDeliveryCost { get; set; }

    public string Currency { get; set; } = "USD";
}

public sealed class OperationalMetrics
{
    public decimal AverageDistance { get; set; }
    public decimal TotalDistance { get; set; }

    // Distribución por rangos de distancia
    public int ShortDistanceOrders { get; set; } // < 5 km
    public int MediumDistanceOrders { get; set; } // 5-15 km
    public int LongDistanceOrders { get; set; } // > 15 km

    public decimal AverageOrdersPerDay { get; set; }
}

public sealed class CustomerMetrics
{
    public int TotalCustomers { get; set; }
    public int NewCustomersToday { get; set; }
    public int NewCustomersWeek { get; set; }
    public int NewCustomersMonth { get; set; }

    public decimal AverageOrdersPerCustomer { get; set; }
    public int ActiveCustomers { get; set; } // Con órdenes en los últimos 30 días
}

public sealed class TrendMetrics
{
    public decimal OrderGrowthRate { get; set; } // vs mes anterior
    public decimal RevenueGrowthRate { get; set; } // vs mes anterior
    public decimal CustomerGrowthRate { get; set; } // vs mes anterior

    public List<DailyMetric> Last7Days { get; set; } = new();
    public List<MonthlyMetric> Last6Months { get; set; } = new();
}

public sealed class DailyMetric
{
    public DateTime Date { get; set; }
    public int OrderCount { get; set; }
    public decimal Revenue { get; set; }
}

public sealed class MonthlyMetric
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = "";
    public int OrderCount { get; set; }
    public decimal Revenue { get; set; }
    public int NewCustomers { get; set; }
}
