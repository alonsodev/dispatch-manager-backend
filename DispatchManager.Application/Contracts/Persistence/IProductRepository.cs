using DispatchManager.Domain.Entities;

namespace DispatchManager.Application.Contracts.Persistence;

/// <summary>
/// Repository contract for Product entity operations in Application layer
/// </summary>
public interface IProductRepository : IRepository<Product>
{
    // Search and filtering
    Task<IReadOnlyList<Product>> SearchByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetActiveProductsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetProductsByPriceRangeAsync(decimal minPrice, decimal maxPrice, CancellationToken cancellationToken = default);

    // UI support queries
    Task<IReadOnlyList<(Guid Id, string Name, decimal UnitPrice)>> GetProductListAsync(CancellationToken cancellationToken = default);

    // Analytics and statistics
    Task<decimal> GetAverageProductPriceAsync(CancellationToken cancellationToken = default);
    Task<(decimal Min, decimal Max)> GetPriceRangeAsync(CancellationToken cancellationToken = default);
   
}
