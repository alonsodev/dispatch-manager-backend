using DispatchManager.Application.Contracts.Persistence;
using DispatchManager.Domain.Entities;
using DispatchManager.Infrastructure.Data;
using DispatchManager.Infrastructure.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace DispatchManager.Infrastructure.Repositories;

public sealed class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(DispatchManagerDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Product>> SearchByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(p => p.Name.Contains(name))
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetActiveProductsAsync(CancellationToken cancellationToken = default)
    {
        // Assuming all products are active for now
        // Could add IsActive property later if needed
        return await DbSet
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetProductsByPriceRangeAsync(
        decimal minPrice,
        decimal maxPrice,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(p => p.UnitPrice >= minPrice && p.UnitPrice <= maxPrice)
            .OrderBy(p => p.UnitPrice)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<(Guid Id, string Name, decimal UnitPrice)>> GetProductListAsync(
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .OrderBy(p => p.Name)
            .Select(p => new ValueTuple<Guid, string, decimal>(p.Id, p.Name, p.UnitPrice))
            .ToListAsync(cancellationToken);
    }

    public async Task<decimal> GetAverageProductPriceAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.AverageAsync(p => p.UnitPrice, cancellationToken);
    }

    public async Task<(decimal Min, decimal Max)> GetPriceRangeAsync(CancellationToken cancellationToken = default)
    {
        var min = await DbSet.MinAsync(p => p.UnitPrice, cancellationToken);
        var max = await DbSet.MaxAsync(p => p.UnitPrice, cancellationToken);
        return (min, max);
    }
}