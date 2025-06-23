using DispatchManager.Application.Contracts.Infrastructure;
using DispatchManager.Application.Models.DTOs;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using System.Text;

namespace DispatchManager.Infrastructure.Services;

public sealed class ReportService : IReportService
{
    private readonly ILogger<ReportService> _logger;

    public ReportService(ILogger<ReportService> logger)
    {
        _logger = logger;

        // Configurar EPPlus para uso no comercial
        ExcelPackage.License.SetNonCommercialPersonal("Alonso Palacios");
    }

    public async Task<byte[]> GenerateOrdersReportExcelAsync(OrdersReportDto report, CancellationToken cancellationToken = default)
    {
        try
        {
            using var package = new ExcelPackage();

            // Hoja 1: Resumen por Cliente
            var customerSheet = package.Workbook.Worksheets.Add("Órdenes por Cliente");
            await CreateCustomerReportSheetAsync(customerSheet, report);

            // Hoja 2: Resumen Global
            if (report.GlobalIntervalCounts.Any())
            {
                var globalSheet = package.Workbook.Worksheets.Add("Resumen General");
                await CreateGlobalSummarySheetAsync(globalSheet, report);
            }

            return await Task.FromResult(package.GetAsByteArray());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generando reporte en excel");
            throw;
        }
    }

    public async Task<string> GenerateOrdersReportCsvAsync(OrdersReportDto report, CancellationToken cancellationToken = default)
    {
        try
        {
            var csv = new StringBuilder();

            // Headers
            csv.AppendLine("Nombre de Cliente,Intervalo de Distancia,Conteo de Órdenes,Órdenes Totales");

            // Data
            foreach (var customerReport in report.CustomerReports)
            {
                foreach (var interval in customerReport.IntervalCounts)
                {
                    csv.AppendLine($"{customerReport.CustomerName},{interval.Key},{interval.Value},{customerReport.TotalOrders}");
                }

                // Total row for customer
                csv.AppendLine($"{customerReport.CustomerName},TOTAL,-,{customerReport.TotalOrders}");
                csv.AppendLine(); // Empty line between customers
            }

            return await Task.FromResult(csv.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generando reporte CSV");
            throw;
        }
    }

    private static async Task CreateCustomerReportSheetAsync(ExcelWorksheet worksheet, OrdersReportDto report)
    {
        // Headers
        worksheet.Cells[1, 1].Value = "Customer Name";
        worksheet.Cells[1, 2].Value = "1-50 km";
        worksheet.Cells[1, 3].Value = "51-200 km";
        worksheet.Cells[1, 4].Value = "201-500 km";
        worksheet.Cells[1, 5].Value = "501-1000 km";
        worksheet.Cells[1, 6].Value = "Total de Órdenes";

        // Style headers
        using (var range = worksheet.Cells[1, 1, 1, 6])
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(79, 129, 189));
            range.Style.Font.Color.SetColor(Color.White);
            range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
        }

        // Data
        int row = 2;
        foreach (var customerReport in report.CustomerReports)
        {
            worksheet.Cells[row, 1].Value = customerReport.CustomerName;
            worksheet.Cells[row, 2].Value = customerReport.IntervalCounts.GetValueOrDefault("1-50 km", 0);
            worksheet.Cells[row, 3].Value = customerReport.IntervalCounts.GetValueOrDefault("51-200 km", 0);
            worksheet.Cells[row, 4].Value = customerReport.IntervalCounts.GetValueOrDefault("201-500 km", 0);
            worksheet.Cells[row, 5].Value = customerReport.IntervalCounts.GetValueOrDefault("501-1000 km", 0);
            worksheet.Cells[row, 6].Value = customerReport.TotalOrders;

            // Style data rows
            using (var range = worksheet.Cells[row, 1, row, 6])
            {
                range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                if (row % 2 == 0)
                {
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(242, 242, 242));
                }
            }

            row++;
        }

        // Totals row
        if (report.CustomerReports.Any())
        {
            worksheet.Cells[row, 1].Value = "TOTAL";
            worksheet.Cells[row, 2].Value = report.CustomerReports.Sum(c => c.IntervalCounts.GetValueOrDefault("1-50 km", 0));
            worksheet.Cells[row, 3].Value = report.CustomerReports.Sum(c => c.IntervalCounts.GetValueOrDefault("51-200 km", 0));
            worksheet.Cells[row, 4].Value = report.CustomerReports.Sum(c => c.IntervalCounts.GetValueOrDefault("201-500 km", 0));
            worksheet.Cells[row, 5].Value = report.CustomerReports.Sum(c => c.IntervalCounts.GetValueOrDefault("501-1000 km", 0));
            worksheet.Cells[row, 6].Value = report.TotalOrders;

            using (var range = worksheet.Cells[row, 1, row, 6])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(217, 217, 217));
                range.Style.Border.BorderAround(ExcelBorderStyle.Medium);
            }
        }

        // Auto-fit columns
        worksheet.Cells.AutoFitColumns();

        await Task.CompletedTask;
    }

    private static async Task CreateGlobalSummarySheetAsync(ExcelWorksheet worksheet, OrdersReportDto report)
    {
        // Headers
        worksheet.Cells[1, 1].Value = "Intervalo de Distancia";
        worksheet.Cells[1, 2].Value = "Conteo de Órdenes";
        worksheet.Cells[1, 3].Value = "Porcentaje";

        // Style headers
        using (var range = worksheet.Cells[1, 1, 1, 3])
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(79, 129, 189));
            range.Style.Font.Color.SetColor(Color.White);
        }

        // Data
        int row = 2;
        var totalCount = report.GlobalIntervalCounts.Values.Sum();

        foreach (var interval in report.GlobalIntervalCounts)
        {
            worksheet.Cells[row, 1].Value = interval.Key;
            worksheet.Cells[row, 2].Value = interval.Value;
            worksheet.Cells[row, 3].Value = totalCount > 0 ? (double)interval.Value / totalCount : 0;
            worksheet.Cells[row, 3].Style.Numberformat.Format = "0.00%";

            row++;
        }

        // Auto-fit columns
        worksheet.Cells.AutoFitColumns();

        await Task.CompletedTask;
    }
}