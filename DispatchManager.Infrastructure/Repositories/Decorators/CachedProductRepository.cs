using DispatchManager.Application.Contracts.Infrastructure;
using DispatchManager.Application.Contracts.Persistence;
using DispatchManager.Application.Services.CacheStrategies;
using DispatchManager.Application.Models.CacheWrappers;
using DispatchManager.Domain.Entities;
using System.Linq.Expressions;

namespace DispatchManager.Infrastructure.Repositories.Decorators;

public sealed class CachedProductRepository : IProductRepository
{
    private readonly IProductRepository _repository;
    private readonly ICacheService _cacheService;
    private readonly TimeSpan _defaultExpiration = TimeSpan.FromMinutes(0); // Productos cambian menos
    private readonly TimeSpan _listExpiration = TimeSpan.FromMinutes(0);
    private readonly TimeSpan _searchExpiration = TimeSpan.FromMinutes(0);
    private readonly TimeSpan _analyticsExpiration = TimeSpan.FromHours(0); // Analytics más estables

    public CachedProductRepository(IProductRepository repository, ICacheService cacheService)
    {
        _repository = repository;
        _cacheService = cacheService;
    }

    // Métodos con cache inteligente
    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cacheKey = CacheKeyBuilder.ProductKey(id);
        var cached = await _cacheService.GetAsync<Product>(cacheKey, cancellationToken);

        if (cached != null)
            return cached;

        var product = await _repository.GetByIdAsync(id, cancellationToken);

        if (product != null)
        {
            var tags = new[] { CacheTagConstants.PRODUCTS, CacheTagConstants.ProductTag(id) };
            await _cacheService.SetWithTagsAsync(cacheKey, product, tags, _defaultExpiration, cancellationToken);
        }

        return product;
    }

    public void Update(Product entity)
    {
        _repository.Update(entity);
        // La invalidación se maneja en UnitOfWork.SaveChangesAsync()
    }

    public void Remove(Product entity)
    {
        _repository.Remove(entity);
        // La invalidación se maneja en UnitOfWork.SaveChangesAsync()
    }

    public void UpdateRange(IEnumerable<Product> entities)
    {
        _repository.UpdateRange(entities);
    }

    public void RemoveRange(IEnumerable<Product> entities)
    {
        _repository.RemoveRange(entities);
    }


    public async Task<IReadOnlyList<Product>> SearchByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var cacheKey = CacheKeyBuilder.ProductSearchKey(name);
        var cached = await _cacheService.GetAsync<IReadOnlyList<Product>>(cacheKey, cancellationToken);

        if (cached != null)
            return cached;

        var products = await _repository.SearchByNameAsync(name, cancellationToken);

        var tags = new[] { CacheTagConstants.PRODUCTS, CacheTagConstants.PRODUCT_LISTS };
        await _cacheService.SetWithTagsAsync(cacheKey, products, tags, _searchExpiration, cancellationToken);

        return products;
    }

    public async Task<IEnumerable<Product>> AddRangeAsync(IEnumerable<Product> entities, CancellationToken cancellationToken = default)
    {
        return await _repository.AddRangeAsync(entities, cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetActiveProductsAsync(CancellationToken cancellationToken = default)
    {
        var cacheKey = CacheKeyBuilder.ActiveProductsKey();
        var cached = await _cacheService.GetAsync<IReadOnlyList<Product>>(cacheKey, cancellationToken);

        if (cached != null)
            return cached;

        var products = await _repository.GetActiveProductsAsync(cancellationToken);

        var tags = new[] { CacheTagConstants.PRODUCTS, CacheTagConstants.PRODUCT_LISTS };
        await _cacheService.SetWithTagsAsync(cacheKey, products, tags, _listExpiration, cancellationToken);

        return products;
    }

    public async Task<IReadOnlyList<Product>> GetProductsByPriceRangeAsync(decimal minPrice, decimal maxPrice, CancellationToken cancellationToken = default)
    {
        var cacheKey = CacheKeyBuilder.ProductsByPriceRangeKey(minPrice, maxPrice);
        var cached = await _cacheService.GetAsync<IReadOnlyList<Product>>(cacheKey, cancellationToken);

        if (cached != null)
            return cached;

        var products = await _repository.GetProductsByPriceRangeAsync(minPrice, maxPrice, cancellationToken);

        var tags = new[] { CacheTagConstants.PRODUCTS, CacheTagConstants.PRODUCT_LISTS };
        await _cacheService.SetWithTagsAsync(cacheKey, products, tags, _searchExpiration, cancellationToken);

        return products;
    }

    public async Task<IReadOnlyList<(Guid Id, string Name, decimal UnitPrice)>> GetProductListAsync(CancellationToken cancellationToken = default)
    {
        var cacheKey = CacheKeyBuilder.ProductListKey();
        var cached = await _cacheService.GetAsync<IReadOnlyList<(Guid Id, string Name, decimal UnitPrice)>>(cacheKey, cancellationToken);

        if (cached != null)
            return cached;

        var productList = await _repository.GetProductListAsync(cancellationToken);

        var tags = new[] { CacheTagConstants.PRODUCTS, CacheTagConstants.PRODUCT_LISTS };
        await _cacheService.SetWithTagsAsync(cacheKey, productList, tags, _listExpiration, cancellationToken);

        return productList;
    }

    // Métodos de analytics con cache más largo
    public async Task<decimal> GetAverageProductPriceAsync(CancellationToken cancellationToken = default)
    {
        var cacheKey = "product_average_price";
        var cached = await _cacheService.GetAsync<AverageProductPriceWrapper>(cacheKey, cancellationToken);

        if (cached != null)
        {
            // Conversión implícita de wrapper a decimal
            return cached;
        }

        var average = await _repository.GetAverageProductPriceAsync(cancellationToken);

        // Conversión implícita de decimal a wrapper
        AverageProductPriceWrapper wrapper = average;
        var tags = new[] { CacheTagConstants.PRODUCTS, CacheTagConstants.REPORTS };
        await _cacheService.SetWithTagsAsync(cacheKey, wrapper, tags, _analyticsExpiration, cancellationToken);

        return average;
    }

    public async Task<(decimal Min, decimal Max)> GetPriceRangeAsync(CancellationToken cancellationToken = default)
    {
        var cacheKey = "product_price_range";
        var cached = await _cacheService.GetAsync<ProductPriceRangeWrapper>(cacheKey, cancellationToken);

        if (cached != null)
        {
            // Conversión implícita de wrapper a tuple
            return cached;
        }

        var priceRange = await _repository.GetPriceRangeAsync(cancellationToken);

        // Conversión implícita de tuple a wrapper
        ProductPriceRangeWrapper wrapper = priceRange;
        var tags = new[] { CacheTagConstants.PRODUCTS, CacheTagConstants.REPORTS };
        await _cacheService.SetWithTagsAsync(cacheKey, wrapper, tags, _analyticsExpiration, cancellationToken);

        return priceRange;
    }

    // Métodos de escritura con invalidación automática
    public async Task<Product> AddAsync(Product entity, CancellationToken cancellationToken = default)
    {
        var result = await _repository.AddAsync(entity, cancellationToken);
        await InvalidateProductCaches(result, CacheInvalidationType.Create);
        return result;
    }

    public async Task RemoveByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _repository.RemoveByIdAsync(id, cancellationToken);
        // Invalidar cache específico
        await _cacheService.InvalidateTagAsync(CacheTagConstants.ProductTag(id), cancellationToken);
        await _cacheService.InvalidateTagAsync(CacheTagConstants.PRODUCTS, cancellationToken);
    }

    // Métodos base sin cache (para operaciones complejas)
    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _repository.GetAllAsync(cancellationToken);

    public async Task<IReadOnlyList<Product>> FindAsync(Expression<Func<Product, bool>> predicate, CancellationToken cancellationToken = default)
        => await _repository.FindAsync(predicate, cancellationToken);

    public async Task<Product?> FirstOrDefaultAsync(Expression<Func<Product, bool>> predicate, CancellationToken cancellationToken = default)
        => await _repository.FirstOrDefaultAsync(predicate, cancellationToken);

    public async Task<bool> ExistsAsync(Expression<Func<Product, bool>> predicate, CancellationToken cancellationToken = default)
        => await _repository.ExistsAsync(predicate, cancellationToken);

    public async Task<int> CountAsync(Expression<Func<Product, bool>>? predicate = null, CancellationToken cancellationToken = default)
        => await _repository.CountAsync(predicate, cancellationToken);

    public async Task<Product?> GetByIdWithIncludesAsync(Guid id, params Expression<Func<Product, object>>[] includes)
        => await _repository.GetByIdWithIncludesAsync(id, includes);

    public async Task<IReadOnlyList<Product>> GetAllWithIncludesAsync(params Expression<Func<Product, object>>[] includes)
        => await _repository.GetAllWithIncludesAsync(includes);

    public async Task<IReadOnlyList<Product>> FindWithIncludesAsync(Expression<Func<Product, bool>> predicate, params Expression<Func<Product, object>>[] includes)
        => await _repository.FindWithIncludesAsync(predicate, includes);

    public async Task<(IReadOnlyList<Product> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, Expression<Func<Product, bool>>? predicate = null, Expression<Func<Product, object>>? orderBy = null, bool ascending = true, CancellationToken cancellationToken = default)
        => await _repository.GetPagedAsync(pageNumber, pageSize, predicate, orderBy, ascending, cancellationToken);

    private async Task InvalidateProductCaches(Product product, CacheInvalidationType operationType)
    {
        var tagsToInvalidate = new List<string>
        {
            CacheTagConstants.PRODUCTS,
            CacheTagConstants.PRODUCT_LISTS,
            CacheTagConstants.REPORTS // Los analytics también se ven afectados
        };

        // Para updates específicos
        if (operationType == CacheInvalidationType.Update)
        {
            tagsToInvalidate.Add(CacheTagConstants.ProductTag(product.Id));
        }

        await _cacheService.InvalidateTagsAsync(tagsToInvalidate.ToArray());
    }
}