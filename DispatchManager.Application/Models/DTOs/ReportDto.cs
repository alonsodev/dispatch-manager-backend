using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DispatchManager.Application.Models.DTOs;

public sealed class OrdersReportDto
{
    public List<CustomerOrdersReportDto> CustomerReports { get; set; } = new();
    public Dictionary<string, int> GlobalIntervalCounts { get; set; } = new();
    public int TotalOrders { get; set; }
    public DateTime GeneratedAt { get; set; }
}

public sealed class CustomerOrdersReportDto
{
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public Dictionary<string, int> IntervalCounts { get; set; } = new();
    public int TotalOrders { get; set; }
}