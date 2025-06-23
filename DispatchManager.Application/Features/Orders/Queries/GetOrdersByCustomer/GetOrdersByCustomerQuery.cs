using DispatchManager.Application.Models.Common;
using DispatchManager.Application.Models.DTOs;
using MediatR;

namespace DispatchManager.Application.Features.Orders.Queries.GetOrdersByCustomer;

public sealed record GetOrdersByCustomerQuery : IRequest<PagedResponse<OrderListDto>>
{
    public Guid CustomerId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string SortBy { get; init; } = "CreatedAt";
    public bool SortDescending { get; init; } = true;
}