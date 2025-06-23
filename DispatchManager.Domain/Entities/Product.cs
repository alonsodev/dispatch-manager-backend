using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DispatchManager.Domain.Exceptions;

namespace DispatchManager.Domain.Entities;

public sealed class Product
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public decimal UnitPrice { get; private set; }
    public string Unit { get; private set; } // kg, units, liters, etc.
    public DateTime CreatedAt { get; private set; }

    private Product() { } // EF Constructor

    private Product(string name, string description, decimal unitPrice, string unit)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        UnitPrice = unitPrice;
        Unit = unit;
        CreatedAt = DateTime.UtcNow;
    }

    public static Product Create(string name, string description, decimal unitPrice, string unit)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Se requiere el nombre del producto");

        if (unitPrice < 0)
            throw new DomainException("El precio unitario no puede ser negativo");

        if (string.IsNullOrWhiteSpace(unit))
            throw new DomainException("Se requiere unidad");

        return new Product(
            name.Trim(),
            description?.Trim() ?? string.Empty,
            unitPrice,
            unit.Trim());
    }

    public void UpdatePrice(decimal newPrice)
    {
        if (newPrice < 0)
            throw new DomainException("El precio unitario no puede ser negativo");

        UnitPrice = newPrice;
    }
}