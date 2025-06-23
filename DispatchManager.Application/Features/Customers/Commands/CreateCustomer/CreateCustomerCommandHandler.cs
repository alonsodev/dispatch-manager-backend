using AutoMapper;
using DispatchManager.Application.Contracts.Persistence;
using DispatchManager.Application.Exceptions;
using DispatchManager.Application.Models.Common;
using DispatchManager.Application.Models.DTOs;
using DispatchManager.Domain.Entities;
using MediatR;

namespace DispatchManager.Application.Features.Customers.Commands.CreateCustomer;

public sealed class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, BaseResponse<CustomerDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateCustomerCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<BaseResponse<CustomerDto>> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        // Verificar que el email no esté ya en uso
        var existingCustomer = await _unitOfWork.Customers.GetByEmailAsync(request.Email, cancellationToken);
        if (existingCustomer != null)
        {
            throw new ValidationException($"Ya existe un cliente con un correo '{request.Email}'");
        }

        try
        {
            var customer = Customer.Create(request.Name, request.Email, request.Phone);

            await _unitOfWork.Customers.AddAsync(customer, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var customerDto = _mapper.Map<CustomerDto>(customer);

            return new BaseResponse<CustomerDto>
            {
                Success = true,
                Message = "Cliente creado satisfactoriamente",
                Data = customerDto
            };
        }
        catch (Domain.Exceptions.DomainException ex)
        {
            throw new ValidationException(ex.Message);
        }
    }
}
