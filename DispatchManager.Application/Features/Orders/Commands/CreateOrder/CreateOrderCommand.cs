using DispatchManager.Application.Models.Common;
using DispatchManager.Application.Models.DTOs;
using MediatR;

namespace DispatchManager.Application.Features.Orders.Commands.CreateOrder;

public sealed record CreateOrderCommand : IRequest<BaseResponse<OrderDto>>
{
    public Guid CustomerId { get; init; }
    public Guid ProductId { get; init; }
    public int Quantity { get; init; }
    public CoordinateDto Origin { get; init; } = new();
    public CoordinateDto Destination { get; init; } = new();
}