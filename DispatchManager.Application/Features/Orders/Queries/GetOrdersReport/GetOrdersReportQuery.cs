using DispatchManager.Application.Models.Common;
using DispatchManager.Application.Models.DTOs;
using MediatR;

namespace DispatchManager.Application.Features.Orders.Queries.GetOrdersReport;

public sealed record GetOrdersReportQuery : IRequest<BaseResponse<OrdersReportDto>>
{
    public Guid? CustomerId { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public bool IncludeGlobalSummary { get; init; } = true;
}