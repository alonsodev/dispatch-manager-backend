using AutoMapper;
using DispatchManager.Application.Contracts.Persistence;
using DispatchManager.Application.Exceptions;
using DispatchManager.Application.Models.Common;
using DispatchManager.Application.Models.DTOs;
using DispatchManager.Domain.Entities;
using MediatR;

namespace DispatchManager.Application.Features.Products.Commands.CreateProduct;

public sealed class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, BaseResponse<ProductDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateProductCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<BaseResponse<ProductDto>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var product = Product.Create(request.Name, request.Description, request.UnitPrice, request.Unit);

            await _unitOfWork.Products.AddAsync(product, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var productDto = _mapper.Map<ProductDto>(product);

            return new BaseResponse<ProductDto>
            {
                Success = true,
                Message = "Producto creado satisfactoriamente",
                Data = productDto
            };
        }
        catch (Domain.Exceptions.DomainException ex)
        {
            throw new ValidationException(ex.Message);
        }
    }
}