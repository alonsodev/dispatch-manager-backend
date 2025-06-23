using DispatchManager.Application.Contracts.Infrastructure;
using DispatchManager.Application.Contracts.Persistence;
using DispatchManager.Application.Models.CacheWrappers;
using DispatchManager.Application.Services.CacheStrategies;
using DispatchManager.Domain.Entities;
using DispatchManager.Domain.Enums;
using System.Linq.Expressions;

namespace DispatchManager.Infrastructure.Repositories.Decorators;

public sealed class CachedOrderRepository : IOrderRepository
{
    private readonly IOrderRepository _repository;
    private readonly ICacheService _cacheService;
    private readonly TimeSpan _defaultExpiration = TimeSpan.FromMinutes(0);
    private readonly TimeSpan _listExpiration = TimeSpan.FromMinutes(0);
    private readonly TimeSpan _searchExpiration = TimeSpan.FromMinutes(0);

    public CachedOrderRepository(IOrderRepository repository, ICacheService cacheService)
    {
        _repository = repository;
        _cacheService = cacheService;
    }

    #region IRepository<Order> Basic CRUD Methods

    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cacheKey = CacheKeyBuilder.OrderKey(id);
        var cached = await _cacheService.GetAsync<Order>(cacheKey, cancellationToken);

        if (cached != null)
            return cached;

        var order = await _repository.GetByIdAsync(id, cancellationToken);

        if (order != null)
        {
            var tags = new[] { CacheTagConstants.ORDERS, CacheTagConstants.OrderTag(id) };
            await _cacheService.SetWithTagsAsync(cacheKey, order, tags, _defaultExpiration, cancellationToken);
        }

        return order;
    }

    public async Task<Order> AddAsync(Order entity, CancellationToken cancellationToken = default)
    {
        var result = await _repository.AddAsync(entity, cancellationToken);
        // La invalidación se maneja en UnitOfWork.SaveChangesAsync()
        return result;
    }

    // ✅ MÉTODOS SINCRÓNICOS - CORRECCIÓN CLAVE
    public void Update(Order entity)
    {
        _repository.Update(entity);
        // La invalidación se maneja en UnitOfWork.SaveChangesAsync()
    }

    public void Remove(Order entity)
    {
        _repository.Remove(entity);
        // La invalidación se maneja en UnitOfWork.SaveChangesAsync()
    }

    public async Task RemoveByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _repository.RemoveByIdAsync(id, cancellationToken);
        // La invalidación se maneja en UnitOfWork.SaveChangesAsync()
    }

    public async Task<IReadOnlyList<Order>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var cacheKey = CacheKeyBuilder.OrderListKey();
        var cached = await _cacheService.GetAsync<IReadOnlyList<Order>>(cacheKey, cancellationToken);

        if (cached != null)
            return cached;

        var orders = await _repository.GetAllAsync(cancellationToken);
        var tags = new[] { CacheTagConstants.ORDERS, CacheTagConstants.ORDER_LISTS };
        await _cacheService.SetWithTagsAsync(cacheKey, orders, tags, _listExpiration, cancellationToken);

        return orders;
    }

    public async Task<IReadOnlyList<Order>> FindAsync(Expression<Func<Order, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _repository.FindAsync(predicate, cancellationToken);
    }

    public async Task<Order?> FirstOrDefaultAsync(Expression<Func<Order, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _repository.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public async Task<bool> ExistsAsync(Expression<Func<Order, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _repository.ExistsAsync(predicate, cancellationToken);
    }

    public async Task<int> CountAsync(Expression<Func<Order, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        var cacheKey = predicate == null
            ? CacheKeyBuilder.OrderCountKey()
            : $"orders:count:{predicate.GetHashCode()}";

        var cached = await _cacheService.GetAsync<CountWrapper>(cacheKey, cancellationToken);
        if (cached != null)
            return cached;

        var count = await _repository.CountAsync(predicate, cancellationToken);
        var tags = new[] { CacheTagConstants.ORDERS, CacheTagConstants.ORDER_LISTS };

        CountWrapper wrapper = count;
        await _cacheService.SetWithTagsAsync(cacheKey, wrapper, tags, _searchExpiration, cancellationToken);

        return count;
    }

    public async Task<Order?> GetByIdWithIncludesAsync(Guid id, params Expression<Func<Order, object>>[] includes)
    {
        return await _repository.GetByIdWithIncludesAsync(id, includes);
    }

    public async Task<IReadOnlyList<Order>> GetAllWithIncludesAsync(params Expression<Func<Order, object>>[] includes)
    {
        return await _repository.GetAllWithIncludesAsync(includes);
    }

    public async Task<IReadOnlyList<Order>> FindWithIncludesAsync(Expression<Func<Order, bool>> predicate, params Expression<Func<Order, object>>[] includes)
    {
        return await _repository.FindWithIncludesAsync(predicate, includes);
    }

    public async Task<(IReadOnlyList<Order> Items, int TotalCount)> GetPagedAsync(
        int pageNumber, int pageSize,
        Expression<Func<Order, bool>>? predicate = null,
        Expression<Func<Order, object>>? orderBy = null,
        bool ascending = true,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = CacheKeyBuilder.OrderPagedKey(pageNumber, pageSize, predicate?.GetHashCode(), orderBy?.GetHashCode(), ascending);
        var cached = await _cacheService.GetAsync<PagedResultWrapper<Order>>(cacheKey, cancellationToken);
        if (cached != null)
            return cached;

        var result = await _repository.GetPagedAsync(pageNumber, pageSize, predicate, orderBy, ascending, cancellationToken);
        var tags = new[] { CacheTagConstants.ORDERS, CacheTagConstants.ORDER_LISTS };

        PagedResultWrapper<Order> wrapper = result;
        await _cacheService.SetWithTagsAsync(cacheKey, wrapper, tags, _searchExpiration, cancellationToken);

        return result;
    }

    public async Task<IEnumerable<Order>> AddRangeAsync(IEnumerable<Order> entities, CancellationToken cancellationToken = default)
    {
        return await _repository.AddRangeAsync(entities, cancellationToken);
    }

    // ✅ MÉTODOS SINCRÓNICOS BULK - CORRECCIÓN CLAVE
    public void UpdateRange(IEnumerable<Order> entities)
    {
        _repository.UpdateRange(entities);
    }

    public void RemoveRange(IEnumerable<Order> entities)
    {
        _repository.RemoveRange(entities);
    }

    #endregion

    #region IOrderRepository Specific Methods

    public async Task<IReadOnlyList<Order>> GetOrdersByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var cacheKey = CacheKeyBuilder.OrdersByCustomerKey(customerId);
        var cached = await _cacheService.GetAsync<IReadOnlyList<Order>>(cacheKey, cancellationToken);

        if (cached != null)
            return cached;

        var orders = await _repository.GetOrdersByCustomerIdAsync(customerId, cancellationToken);
        var tags = new[] {
            CacheTagConstants.ORDERS,
            CacheTagConstants.ORDER_LISTS,
            CacheTagConstants.CustomerTag(customerId)
        };
        await _cacheService.SetWithTagsAsync(cacheKey, orders, tags, _listExpiration, cancellationToken);

        return orders;
    }

    public async Task<IReadOnlyList<Order>> GetOrdersByCustomerIdWithDetailsAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await _repository.GetOrdersByCustomerIdWithDetailsAsync(customerId, cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var cacheKey = CacheKeyBuilder.OrdersByDateRangeKey(startDate, endDate);
        var cached = await _cacheService.GetAsync<IReadOnlyList<Order>>(cacheKey, cancellationToken);

        if (cached != null)
            return cached;

        var orders = await _repository.GetOrdersByDateRangeAsync(startDate, endDate, cancellationToken);
        var tags = new[] { CacheTagConstants.ORDERS, CacheTagConstants.ORDER_LISTS };
        await _cacheService.SetWithTagsAsync(cacheKey, orders, tags, _listExpiration, cancellationToken);

        return orders;
    }

    public async Task<IReadOnlyList<Order>> GetOrdersByStatusAsync(OrderStatus status, CancellationToken cancellationToken = default)
    {
        var cacheKey = CacheKeyBuilder.OrdersByStatusKey(status.ToString());
        var cached = await _cacheService.GetAsync<IReadOnlyList<Order>>(cacheKey, cancellationToken);

        if (cached != null)
            return cached;

        var orders = await _repository.GetOrdersByStatusAsync(status, cancellationToken);
        var tags = new[] { CacheTagConstants.ORDERS, CacheTagConstants.ORDER_LISTS };
        await _cacheService.SetWithTagsAsync(cacheKey, orders, tags, _listExpiration, cancellationToken);

        return orders;
    }

    public async Task<IReadOnlyList<Order>> GetOrdersWithDetailsAsync(CancellationToken cancellationToken = default)
    {
        return await _repository.GetOrdersWithDetailsAsync(cancellationToken);
    }

    public async Task<Dictionary<string, int>> GetOrderCountByDistanceIntervalAsync(CancellationToken cancellationToken = default)
    {
        return await _repository.GetOrderCountByDistanceIntervalAsync(cancellationToken);
    }

    public async Task<Dictionary<string, int>> GetOrderCountByDistanceIntervalForCustomerAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await _repository.GetOrderCountByDistanceIntervalForCustomerAsync(customerId, cancellationToken);
    }

    public async Task<IReadOnlyList<(Guid CustomerId, string CustomerName, string DistanceInterval, int OrderCount)>> GetOrderCountByCustomerAndIntervalAsync(CancellationToken cancellationToken = default)
    {
        return await _repository.GetOrderCountByCustomerAndIntervalAsync(cancellationToken);
    }

    public async Task<Order?> GetOrderWithFullDetailsAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await _repository.GetOrderWithFullDetailsAsync(orderId, cancellationToken);
    }

    public async Task<bool> HasOrdersInProgressForCustomerAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await _repository.HasOrdersInProgressForCustomerAsync(customerId, cancellationToken);
    }

    #endregion
}
