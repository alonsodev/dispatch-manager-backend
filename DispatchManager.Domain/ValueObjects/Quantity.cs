using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DispatchManager.Domain.Exceptions;

namespace DispatchManager.Domain.ValueObjects;

public sealed class Quantity : IEquatable<Quantity>, IComparable<Quantity>
{
    public int Value { get; }

    private Quantity(int value)
    {
        Value = value;
    }

    public static Quantity Create(int value)
    {
        if (value <= 0)
            throw new DomainException("La cantidad debe ser mayor que cero");

        return new Quantity(value);
    }

    /// <summary>
    /// Incrementa la cantidad en el valor especificado
    /// </summary>
    /// <param name="increment">Valor a incrementar (debe ser positivo)</param>
    /// <returns>Nueva instancia con la cantidad incrementada</returns>
    public Quantity Add(int increment)
    {
        if (increment <= 0)
            throw new DomainException("El incremento debe ser positivo");

        return new Quantity(Value + increment);
    }

    /// <summary>
    /// Reduce la cantidad en el valor especificado
    /// </summary>
    /// <param name="decrement">Valor a reducir (debe ser positivo y menor que el valor actual)</param>
    /// <returns>Nueva instancia con la cantidad reducida</returns>
    public Quantity Subtract(int decrement)
    {
        if (decrement <= 0)
            throw new DomainException("El decremento debe ser positivo");

        if (decrement >= Value)
            throw new DomainException("El decremento no puede ser mayor o igual al valor actual");

        return new Quantity(Value - decrement);
    }

    public static explicit operator Quantity(int value) => Create(value);

    public bool Equals(Quantity? other) => other is not null && Value == other.Value;

    public override bool Equals(object? obj) => Equals(obj as Quantity);

    public override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(Quantity? left, Quantity? right) =>
        EqualityComparer<Quantity>.Default.Equals(left, right);

    public static bool operator !=(Quantity? left, Quantity? right) => !(left == right);

    public static implicit operator int(Quantity quantity) => quantity.Value;

    public int CompareTo(Quantity? other) =>
        other is null ? 1 : Value.CompareTo(other.Value);

    public static bool operator <(Quantity left, Quantity right) =>
        left.CompareTo(right) < 0;

    public static bool operator >(Quantity left, Quantity right) =>
        left.CompareTo(right) > 0;

    public static bool operator <=(Quantity left, Quantity right) =>
        left.CompareTo(right) <= 0;

    public static bool operator >=(Quantity left, Quantity right) =>
        left.CompareTo(right) >= 0;
}
