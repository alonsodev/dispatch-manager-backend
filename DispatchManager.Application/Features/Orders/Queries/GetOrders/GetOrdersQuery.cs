using DispatchManager.Application.Models.Common;
using DispatchManager.Application.Models.DTOs;
using DispatchManager.Domain.Enums;
using MediatR;

namespace DispatchManager.Application.Features.Orders.Queries.GetOrders;

public sealed record GetOrdersQuery : IRequest<PagedResponse<OrderListDto>>
{
    public string? SearchTerm { get; init; }
    public OrderStatus? Status { get; init; }
    public Guid? CustomerId { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 25;
    public string SortBy { get; init; } = "CreatedAt";
    public string SortDirection { get; init; } = "desc";
    public bool SortDescending => SortDirection.ToLowerInvariant() == "desc";
}