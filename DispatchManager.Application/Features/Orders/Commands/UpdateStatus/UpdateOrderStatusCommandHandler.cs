using DispatchManager.Application.Contracts.Infrastructure;
using DispatchManager.Application.Contracts.Persistence;
using DispatchManager.Application.Exceptions;
using DispatchManager.Application.Models.Common;
using DispatchManager.Domain.Entities;
using MediatR;

namespace DispatchManager.Application.Features.Orders.Commands.UpdateOrderStatus;

public sealed class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand, BaseResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;

    public UpdateOrderStatusCommandHandler(
        IUnitOfWork unitOfWork,
        IEmailService emailService)
    {
        _unitOfWork = unitOfWork;
        _emailService = emailService;
    }

    public async Task<BaseResponse> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await _unitOfWork.Orders.GetOrderWithFullDetailsAsync(request.OrderId, cancellationToken);
        if (order == null)
        {
            throw new NotFoundException(nameof(Order), request.OrderId);
        }

        try
        {
            order.UpdateStatus(request.NewStatus);
            _unitOfWork.Orders.Update(order);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Enviar notificación por email
            _ = Task.Run(async () =>
            {
                try
                {
                    await _emailService.SendOrderStatusUpdatedNotificationAsync(
                        order.Customer.Email,
                        order.Customer.Name,
                        $"Order #{order.Id}",
                        request.NewStatus.ToString(),
                        cancellationToken);
                }
                catch (Exception ex)
                {
                    // Log error but don't fail the status update
                    // TODO: Add proper logging
                }
            }, cancellationToken);

            return new BaseResponse
            {
                Success = true,
                Message = $"Order status updated to {request.NewStatus}"
            };
        }
        catch (Domain.Exceptions.DomainException ex)
        {
            throw new BusinessRuleException(ex.Message);
        }
    }
}
