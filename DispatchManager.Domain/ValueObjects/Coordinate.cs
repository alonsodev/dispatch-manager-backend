using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DispatchManager.Domain.Exceptions;

namespace DispatchManager.Domain.ValueObjects;

/// <summary>
/// Representa una coordenada geográfica con validaciones de latitud y longitud
/// </summary>
public sealed class Coordinate : IEquatable<Coordinate>
{
    public double Latitude { get; }
    public double Longitude { get; }

    private Coordinate(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }

    /// <summary>
    /// Crea una nueva coordenada validando rangos geográficos
    /// </summary>
    /// <param name="latitude">Latitud (-90 a 90 grados)</param>
    /// <param name="longitude">Longitud (-180 a 180 grados)</param>
    /// <returns>Nueva instancia de Coordinate</returns>
    public static Coordinate Create(double latitude, double longitude)
    {
        if (latitude < -90 || latitude > 90)
            throw new DomainException("La latitud debe estar entre -90 y 90 grados");

        if (longitude < -180 || longitude > 180)
            throw new DomainException("La longitud debe estar entre -180 y 180 grados");

        return new Coordinate(latitude, longitude);
    }

    /// <summary>
    /// Calcula si dos coordenadas son equivalentes (tolerancia de 0.0001 grados)
    /// </summary>
    public bool Equals(Coordinate? other)
    {
        if (other is null) return false;
        return Math.Abs(Latitude - other.Latitude) < 0.0001 &&
                Math.Abs(Longitude - other.Longitude) < 0.0001;
    }

    public override bool Equals(object? obj) => Equals(obj as Coordinate);

    public override int GetHashCode() => HashCode.Combine(
        Math.Round(Latitude, 4),
        Math.Round(Longitude, 4));

    public override string ToString() => $"({Latitude:F6}, {Longitude:F6})";

    public static bool operator ==(Coordinate? left, Coordinate? right) =>
        EqualityComparer<Coordinate>.Default.Equals(left, right);

    public static bool operator !=(Coordinate? left, Coordinate? right) => !(left == right);
}