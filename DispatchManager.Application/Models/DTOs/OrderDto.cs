using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DispatchManager.Domain.Enums;

namespace DispatchManager.Application.Models.DTOs;

public sealed class OrderDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public CoordinateDto Origin { get; set; } = new();
    public CoordinateDto Destination { get; set; } = new();
    public double DistanceKm { get; set; }
    public string DistanceInterval { get; set; } = string.Empty;
    public decimal CostAmount { get; set; }
    public string CostCurrency { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public sealed class CreateOrderDto
{
    public Guid CustomerId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public CoordinateDto Origin { get; set; } = new();
    public CoordinateDto Destination { get; set; } = new();
}

public sealed class OrderListDto
{
    public Guid Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public double DistanceKm { get; set; }
    public decimal CostAmount { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class CoordinateDto
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}