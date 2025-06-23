using DispatchManager.Application.Models.DTOs;

namespace DispatchManager.Application.Contracts.Infrastructure;

public interface IReportService
{
    Task<byte[]> GenerateOrdersReportExcelAsync(OrdersReportDto report, CancellationToken cancellationToken = default);
    Task<string> GenerateOrdersReportCsvAsync(OrdersReportDto report, CancellationToken cancellationToken = default);
}