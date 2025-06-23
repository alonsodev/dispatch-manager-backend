using AutoMapper;
using DispatchManager.Application.Models.DTOs;
using DispatchManager.Domain.Entities;
using DispatchManager.Domain.ValueObjects;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DispatchManager.Application.Mappings;

public sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Customer, CustomerDto>();
        CreateMap<Customer, CustomerListDto>();
        CreateMap<CreateCustomerDto, Customer>()
            .ConstructUsing(src => Customer.Create(src.Name, src.Email, src.Phone));

        CreateMap<Product, ProductDto>();
        CreateMap<Product, ProductListDto>();
        CreateMap<CreateProductDto, Product>()
            .ConstructUsing(src => Product.Create(src.Name, src.Description, src.UnitPrice, src.Unit));

        CreateMap<Order, OrderDto>()
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer.Name))
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
            .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity.Value))
            .ForMember(dest => dest.DistanceKm, opt => opt.MapFrom(src => src.Distance.Kilometers))
            .ForMember(dest => dest.DistanceInterval, opt => opt.MapFrom(src => src.Distance.GetCostInterval()))
            .ForMember(dest => dest.CostAmount, opt => opt.MapFrom(src => src.Cost.Amount))
            .ForMember(dest => dest.CostCurrency, opt => opt.MapFrom(src => src.Cost.Currency));

        CreateMap<Order, OrderListDto>()
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer.Name))
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
            .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity.Value))
            .ForMember(dest => dest.DistanceKm, opt => opt.MapFrom(src => src.Distance.Kilometers))
            .ForMember(dest => dest.CostAmount, opt => opt.MapFrom(src => src.Cost.Amount));

        CreateMap<Coordinate, CoordinateDto>();
        CreateMap<CoordinateDto, Coordinate>()
            .ConstructUsing(src => Coordinate.Create(src.Latitude, src.Longitude));
    }
}