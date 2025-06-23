using DispatchManager.Application.Models.Common;
using DispatchManager.Domain.Enums;
using MediatR;

namespace DispatchManager.Application.Features.Orders.Commands.UpdateOrderStatus;

public sealed record UpdateOrderStatusCommand : IRequest<BaseResponse>
{
    public Guid OrderId { get; init; }
    public OrderStatus NewStatus { get; init; }
}
