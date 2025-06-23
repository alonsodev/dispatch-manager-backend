namespace DispatchManager.Application.Models.CacheWrappers;

/// <summary>
/// Wrapper para cachear el precio promedio de productos
/// Permite usar decimal en cache que requiere reference types
/// </summary>
public sealed class AverageProductPriceWrapper
{
    public decimal AveragePrice { get; init; }

    public AverageProductPriceWrapper(decimal averagePrice)
    {
        AveragePrice = averagePrice;
    }

    // Conversión implícita desde wrapper a decimal
    public static implicit operator decimal(AverageProductPriceWrapper wrapper)
        => wrapper.AveragePrice;

    // Conversión implícita desde decimal a wrapper
    public static implicit operator AverageProductPriceWrapper(decimal averagePrice)
        => new(averagePrice);

    public override string ToString() => AveragePrice.ToString("C");

    public override bool Equals(object? obj)
        => obj is AverageProductPriceWrapper other && AveragePrice == other.AveragePrice;

    public override int GetHashCode() => AveragePrice.GetHashCode();
}

/// <summary>
/// Wrapper para cachear el rango de precios de productos (min, max)
/// Permite usar tuple de decimals en cache que requiere reference types
/// </summary>
public sealed class ProductPriceRangeWrapper
{
    public decimal Min { get; init; }
    public decimal Max { get; init; }

    public ProductPriceRangeWrapper(decimal min, decimal max)
    {
        Min = min;
        Max = max;
    }

    // Método Deconstruct para uso natural con tuples
    public void Deconstruct(out decimal min, out decimal max)
    {
        min = Min;
        max = Max;
    }

    // Conversión implícita desde wrapper a tuple
    public static implicit operator (decimal Min, decimal Max)(ProductPriceRangeWrapper wrapper)
        => (wrapper.Min, wrapper.Max);

    // Conversión implícita desde tuple a wrapper
    public static implicit operator ProductPriceRangeWrapper((decimal Min, decimal Max) range)
        => new(range.Min, range.Max);

    public override string ToString() => $"Min: {Min:C}, Max: {Max:C}";

    public override bool Equals(object? obj)
        => obj is ProductPriceRangeWrapper other && Min == other.Min && Max == other.Max;

    public override int GetHashCode() => HashCode.Combine(Min, Max);
}