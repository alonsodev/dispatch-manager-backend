using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DispatchManager.Domain.Exceptions;

namespace DispatchManager.Domain.ValueObjects;

public sealed class Distance : IEquatable<Distance>, IComparable<Distance>
{
    public double Kilometers { get; }

    private Distance(double kilometers)
    {
        Kilometers = kilometers;
    }

    public static Distance Create(double kilometers)
    {
        if (kilometers < 1)
            throw new DomainException("La distancia no puede ser inferior a 1 kilómetro");

        if (kilometers > 1000)
            throw new DomainException("La distancia no puede exceder los 1000 kilómetros");

        return new Distance(Math.Round(kilometers, 2));
    }

    /// <summary>
    /// Obtiene el intervalo de costo al que pertenece esta distancia
    /// </summary>
    /// <returns>Descripción del intervalo</returns>
    public string GetCostInterval()
    {
        return Kilometers switch
        {
            >= 1 and <= 50 => "1-50 km",
            >= 51 and <= 200 => "51-200 km",
            >= 201 and <= 500 => "201-500 km",
            >= 501 and <= 1000 => "501-1000 km",
            _ => "Invalid range"
        };
    }

    public static bool operator <(Distance left, Distance right) =>
    left.CompareTo(right) < 0;

    public static bool operator >(Distance left, Distance right) =>
        left.CompareTo(right) > 0;

    public static bool operator <=(Distance left, Distance right) =>
        left.CompareTo(right) <= 0;

    public static bool operator >=(Distance left, Distance right) =>
        left.CompareTo(right) >= 0;

    public bool IsInRange(double min, double max) =>
        Kilometers >= min && Kilometers <= max;

    public bool Equals(Distance? other) =>
        other is not null && Math.Abs(Kilometers - other.Kilometers) < 0.01;

    public override bool Equals(object? obj) => Equals(obj as Distance);

    public override int GetHashCode() => Kilometers.GetHashCode();

    public int CompareTo(Distance? other) =>
        other is null ? 1 : Kilometers.CompareTo(other.Kilometers);

    public static bool operator ==(Distance? left, Distance? right) =>
        EqualityComparer<Distance>.Default.Equals(left, right);

    public static bool operator !=(Distance? left, Distance? right) => !(left == right);
}
