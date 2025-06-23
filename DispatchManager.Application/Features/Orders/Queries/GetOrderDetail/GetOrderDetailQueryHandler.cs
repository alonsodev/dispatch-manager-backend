using AutoMapper;
using DispatchManager.Application.Contracts.Infrastructure;
using DispatchManager.Application.Contracts.Persistence;
using DispatchManager.Application.Exceptions;
using DispatchManager.Application.Models.Common;
using DispatchManager.Application.Models.DTOs;
using DispatchManager.Domain.Entities;
using MediatR;

namespace DispatchManager.Application.Features.Orders.Queries.GetOrderDetail;

public sealed class GetOrderDetailQueryHandler : IRequestHandler<GetOrderDetailQuery, BaseResponse<OrderDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICacheService _cacheService;

    public GetOrderDetailQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _cacheService = cacheService;
    }

    public async Task<BaseResponse<OrderDto>> Handle(GetOrderDetailQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"order_detail_{request.OrderId}";
        var cachedOrder = await _cacheService.GetAsync<OrderDto>(cacheKey, cancellationToken);

        if (cachedOrder != null)
        {
            return new BaseResponse<OrderDto>
            {
                Success = true,
                Message = "Orden recuperado con éxito",
                Data = cachedOrder
            };
        }

        var order = await _unitOfWork.Orders.GetOrderWithFullDetailsAsync(request.OrderId, cancellationToken);
        if (order == null)
        {
            throw new NotFoundException(nameof(Order), request.OrderId);
        }

        var orderDto = _mapper.Map<OrderDto>(order);

        // Cache por 30 minutos
        await _cacheService.SetAsync(cacheKey, orderDto, TimeSpan.FromMinutes(30), cancellationToken);

        return new BaseResponse<OrderDto>
        {
            Success = true,
            Message = "Orden recuperado con éxito",
            Data = orderDto
        };
    }
}