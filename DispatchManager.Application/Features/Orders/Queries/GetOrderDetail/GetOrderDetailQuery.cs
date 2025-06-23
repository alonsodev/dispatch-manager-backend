using DispatchManager.Application.Models.Common;
using DispatchManager.Application.Models.DTOs;
using MediatR;

namespace DispatchManager.Application.Features.Orders.Queries.GetOrderDetail;

public sealed record GetOrderDetailQuery : IRequest<BaseResponse<OrderDto>>
{
    public Guid OrderId { get; init; }
}
