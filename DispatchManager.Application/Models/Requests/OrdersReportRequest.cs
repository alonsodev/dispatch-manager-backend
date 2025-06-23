namespace DispatchManager.Application.Models.Requests;

public sealed class OrdersReportRequest
{
    public Guid? CustomerId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? DistanceInterval { get; set; }
    public bool IncludeGlobalSummary { get; set; } = true;
    public string ExportFormat { get; set; } = "excel"; // excel, csv
}

public sealed class GetOrdersRequest
{
    public Guid? CustomerId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Status { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
}