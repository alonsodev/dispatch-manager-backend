using AutoMapper;
using DispatchManager.Application.Contracts.Infrastructure;
using DispatchManager.Application.Contracts.Persistence;
using DispatchManager.Application.Exceptions;
using DispatchManager.Application.Models.Common;
using DispatchManager.Application.Models.DTOs;
using DispatchManager.Domain.Entities;
using DispatchManager.Domain.Services;
using DispatchManager.Domain.ValueObjects;
using MediatR;

namespace DispatchManager.Application.Features.Orders.Commands.CreateOrder;

public sealed class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, BaseResponse<OrderDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IDistanceCalculationService _distanceService;
    private readonly ICostCalculationService _costService;
    private readonly IEmailService _emailService;

    public CreateOrderCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IDistanceCalculationService distanceService,
        ICostCalculationService costService,
        IEmailService emailService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _distanceService = distanceService;
        _costService = costService;
        _emailService = emailService;
    }

    public async Task<BaseResponse<OrderDto>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // Verificar que el cliente existe
        var customer = await _unitOfWork.Customers.GetByIdAsync(request.CustomerId, cancellationToken);
        if (customer == null)
        {
            throw new NotFoundException(nameof(Customer), request.CustomerId);
        }

        // Verificar que el producto existe
        var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId, cancellationToken);
        if (product == null)
        {
            throw new NotFoundException(nameof(Product), request.ProductId);
        }

        try
        {
            // Crear value objects
            var origin = Coordinate.Create(request.Origin.Latitude, request.Origin.Longitude);
            var destination = Coordinate.Create(request.Destination.Latitude, request.Destination.Longitude);
            var quantity = Quantity.Create(request.Quantity);

            // Calcular distancia
            var distance = _distanceService.CalculateDistance(origin, destination);

            // Calcular costo
            var cost = _costService.CalculateCost(distance);

            // Crear la orden
            var order = Order.Create(
                request.CustomerId,
                request.ProductId,
                quantity,
                origin,
                destination,
                distance,
                cost);

            // Guardar en la base de datos
            await _unitOfWork.Orders.AddAsync(order, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Cargar la orden con sus relaciones para el mapeo
            var savedOrder = await _unitOfWork.Orders.GetOrderWithFullDetailsAsync(order.Id, cancellationToken);

            var orderDto = _mapper.Map<OrderDto>(savedOrder);

            // Enviar notificación por email (sin bloquear la respuesta)
            _ = Task.Run(async () =>
            {
                try
                {
                    await _emailService.SendOrderCreatedNotificationAsync(
                        customer.Email,
                        customer.Name,
                        $"Order #{order.Id} for {product.Name} (Qty: {quantity}) - Distance: {distance.Kilometers:F2}km - Cost: ${cost.Amount}",
                        cancellationToken);
                }
                catch (Exception ex)
                {
                    // Log error but don't fail the order creation
                    // TODO: Add proper logging
                }
            }, cancellationToken);

            return new BaseResponse<OrderDto>
            {
                Success = true,
                Message = "Order created successfully",
                Data = orderDto
            };
        }
        catch (Domain.Exceptions.DomainException ex)
        {
            throw new BusinessRuleException(ex.Message);
        }
    }
}