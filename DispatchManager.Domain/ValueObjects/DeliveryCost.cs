using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DispatchManager.Domain.Exceptions;

namespace DispatchManager.Domain.ValueObjects;

public sealed class DeliveryCost : IEquatable<DeliveryCost>
{
    public decimal Amount { get; }
    public string Currency { get; }

    private DeliveryCost(decimal amount, string currency = "USD")
    {
        Amount = amount;
        Currency = currency;
    }

    public static DeliveryCost Create(decimal amount)
    {
        if (amount < 0)
            throw new DomainException("El costo de entrega no puede ser negativo");

        return new DeliveryCost(amount);
    }

    public static DeliveryCost FromDistance(Distance distance)
    {
        var amount = distance.Kilometers switch
        {
            >= 1 and <= 50 => 100m,
            >= 51 and <= 200 => 300m,
            >= 201 and <= 500 => 1000m,
            >= 501 and <= 1000 => 1500m,
            _ => throw new DomainException($"Rango de distancia no válido: {distance.Kilometers} km")
        };

        return new DeliveryCost(amount);
    }

    /// <summary>
    /// Aplica un descuento al costo actual
    /// </summary>
    /// <param name="discountPercentage">Porcentaje de descuento (0-100)</param>
    /// <returns>Nuevo costo con descuento aplicado</returns>
    public DeliveryCost ApplyDiscount(decimal discountPercentage)
    {
        if (discountPercentage < 0 || discountPercentage > 100)
            throw new DomainException("Discount percentage must be between 0 and 100");

        var discountAmount = Amount * (discountPercentage / 100);
        var newAmount = Amount - discountAmount;

        return new DeliveryCost(newAmount, Currency);
    }

    public bool Equals(DeliveryCost? other) =>
        other is not null && Amount == other.Amount && Currency == other.Currency;

    public override bool Equals(object? obj) => Equals(obj as DeliveryCost);

    public override int GetHashCode() => HashCode.Combine(Amount, Currency);

    public static bool operator ==(DeliveryCost? left, DeliveryCost? right) =>
        EqualityComparer<DeliveryCost>.Default.Equals(left, right);

    public static bool operator !=(DeliveryCost? left, DeliveryCost? right) => !(left == right);
}