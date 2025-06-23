using DispatchManager.Application.Models.Common;
using DispatchManager.Application.Models.DTOs;
using MediatR;

namespace DispatchManager.Application.Features.Orders.Queries.GetOrdersMetrics;

public sealed record GetOrdersMetricsQuery : IRequest<BaseResponse<OrdersMetricsDto>>
{
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public bool IncludeTrends { get; init; } = true;
}