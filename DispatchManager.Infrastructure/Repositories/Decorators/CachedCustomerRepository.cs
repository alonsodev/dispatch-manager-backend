using DispatchManager.Application.Contracts.Infrastructure;
using DispatchManager.Application.Contracts.Persistence;
using DispatchManager.Application.Services.CacheStrategies;
using DispatchManager.Domain.Entities;
using System.Linq.Expressions;

namespace DispatchManager.Infrastructure.Repositories.Decorators;

public sealed class CachedCustomerRepository : ICustomerRepository
{
    private readonly ICustomerRepository _repository;
    private readonly ICacheService _cacheService;
    private readonly TimeSpan _defaultExpiration = TimeSpan.FromMinutes(0);
    private readonly TimeSpan _listExpiration = TimeSpan.FromMinutes(0);
    private readonly TimeSpan _searchExpiration = TimeSpan.FromMinutes(0);

    public CachedCustomerRepository(ICustomerRepository repository, ICacheService cacheService)
    {
        _repository = repository;
        _cacheService = cacheService;
    }

    // Métodos con cache inteligente
    public async Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cacheKey = CacheKeyBuilder.CustomerKey(id);
        var cached = await _cacheService.GetAsync<Customer>(cacheKey, cancellationToken);

        if (cached != null)
            return cached;

        var customer = await _repository.GetByIdAsync(id, cancellationToken);

        if (customer != null)
        {
            var tags = new[] { CacheTagConstants.CUSTOMERS, CacheTagConstants.CustomerTag(id) };
            await _cacheService.SetWithTagsAsync(cacheKey, customer, tags, _defaultExpiration, cancellationToken);
        }

        return customer;
    }

    public async Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var cacheKey = CacheKeyBuilder.CustomerByEmailKey(email);
        var cached = await _cacheService.GetAsync<Customer>(cacheKey, cancellationToken);

        if (cached != null)
            return cached;

        var customer = await _repository.GetByEmailAsync(email, cancellationToken);

        if (customer != null)
        {
            var tags = new[] { CacheTagConstants.CUSTOMERS, CacheTagConstants.CustomerTag(customer.Id) };
            await _cacheService.SetWithTagsAsync(cacheKey, customer, tags, _defaultExpiration, cancellationToken);
        }

        return customer;
    }

    public async Task<IReadOnlyList<Customer>> GetCustomersWithOrdersAsync(CancellationToken cancellationToken = default)
    {
        var cacheKey = CacheKeyBuilder.CustomersWithOrdersKey();
        var cached = await _cacheService.GetAsync<IReadOnlyList<Customer>>(cacheKey, cancellationToken);

        if (cached != null)
            return cached;

        var customers = await _repository.GetCustomersWithOrdersAsync(cancellationToken);

        var tags = new[] { CacheTagConstants.CUSTOMERS, CacheTagConstants.CUSTOMER_LISTS };
        await _cacheService.SetWithTagsAsync(cacheKey, customers, tags, _listExpiration, cancellationToken);

        return customers;
    }

    public async Task<IReadOnlyList<Customer>> SearchByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var cacheKey = CacheKeyBuilder.CustomerSearchKey(name);
        var cached = await _cacheService.GetAsync<IReadOnlyList<Customer>>(cacheKey, cancellationToken);

        if (cached != null)
            return cached;

        var customers = await _repository.SearchByNameAsync(name, cancellationToken);

        var tags = new[] { CacheTagConstants.CUSTOMERS, CacheTagConstants.CUSTOMER_LISTS };
        await _cacheService.SetWithTagsAsync(cacheKey, customers, tags, _searchExpiration, cancellationToken);

        return customers;
    }

    public async Task<IReadOnlyList<(Guid Id, string Name)>> GetCustomerListAsync(CancellationToken cancellationToken = default)
    {
        var cacheKey = CacheKeyBuilder.CustomerListKey();
        var cached = await _cacheService.GetAsync<IReadOnlyList<(Guid Id, string Name)>>(cacheKey, cancellationToken);

        if (cached != null)
            return cached;

        var customerList = await _repository.GetCustomerListAsync(cancellationToken);

        var tags = new[] { CacheTagConstants.CUSTOMERS, CacheTagConstants.CUSTOMER_LISTS };
        await _cacheService.SetWithTagsAsync(cacheKey, customerList, tags, _listExpiration, cancellationToken);

        return customerList;
    }

    // Método sin cache (validación rápida)
    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        // Este método es típicamente para validaciones, mejor sin cache para consistencia
        return await _repository.ExistsByEmailAsync(email, cancellationToken);
    }

    // Métodos de escritura con invalidación automática
    public async Task<Customer> AddAsync(Customer entity, CancellationToken cancellationToken = default)
    {
        var result = await _repository.AddAsync(entity, cancellationToken);
        await InvalidateCustomerCaches(result, CacheInvalidationType.Create);
        return result;
    }
    public async Task<IEnumerable<Customer>> AddRangeAsync(IEnumerable<Customer> entities, CancellationToken cancellationToken = default)
    {
        return await _repository.AddRangeAsync(entities, cancellationToken);
    }

    public void Update(Customer entity)
    {
        _repository.Update(entity);
        // La invalidación se maneja en UnitOfWork.SaveChangesAsync
    }

    public void Remove(Customer entity)
    {
        _repository.Remove(entity);
        // La invalidación se maneja en UnitOfWork.SaveChangesAsync
    }

    public void UpdateRange(IEnumerable<Customer> entities)
    {
        _repository.UpdateRange(entities);
    }

    public void RemoveRange(IEnumerable<Customer> entities)
    {
        _repository.RemoveRange(entities);
    }

    public async Task RemoveByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _repository.RemoveByIdAsync(id, cancellationToken);
        // Invalidar cache específico
        await _cacheService.InvalidateTagAsync(CacheTagConstants.CustomerTag(id), cancellationToken);
        await _cacheService.InvalidateTagAsync(CacheTagConstants.CUSTOMERS, cancellationToken);
    }

    // Métodos base sin cache (para operaciones complejas)
    public async Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _repository.GetAllAsync(cancellationToken);

    public async Task<IReadOnlyList<Customer>> FindAsync(Expression<Func<Customer, bool>> predicate, CancellationToken cancellationToken = default)
        => await _repository.FindAsync(predicate, cancellationToken);

    public async Task<Customer?> FirstOrDefaultAsync(Expression<Func<Customer, bool>> predicate, CancellationToken cancellationToken = default)
        => await _repository.FirstOrDefaultAsync(predicate, cancellationToken);

    public async Task<bool> ExistsAsync(Expression<Func<Customer, bool>> predicate, CancellationToken cancellationToken = default)
        => await _repository.ExistsAsync(predicate, cancellationToken);

    public async Task<int> CountAsync(Expression<Func<Customer, bool>>? predicate = null, CancellationToken cancellationToken = default)
        => await _repository.CountAsync(predicate, cancellationToken);

    public async Task<Customer?> GetByIdWithIncludesAsync(Guid id, params Expression<Func<Customer, object>>[] includes)
        => await _repository.GetByIdWithIncludesAsync(id, includes);

    public async Task<IReadOnlyList<Customer>> GetAllWithIncludesAsync(params Expression<Func<Customer, object>>[] includes)
        => await _repository.GetAllWithIncludesAsync(includes);

    public async Task<IReadOnlyList<Customer>> FindWithIncludesAsync(Expression<Func<Customer, bool>> predicate, params Expression<Func<Customer, object>>[] includes)
        => await _repository.FindWithIncludesAsync(predicate, includes);

    public async Task<(IReadOnlyList<Customer> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, Expression<Func<Customer, bool>>? predicate = null, Expression<Func<Customer, object>>? orderBy = null, bool ascending = true, CancellationToken cancellationToken = default)
        => await _repository.GetPagedAsync(pageNumber, pageSize, predicate, orderBy, ascending, cancellationToken);

    private async Task InvalidateCustomerCaches(Customer customer, CacheInvalidationType operationType)
    {
        var tagsToInvalidate = new List<string>
        {
            CacheTagConstants.CUSTOMERS,
            CacheTagConstants.CUSTOMER_LISTS
        };

        // Para updates específicos
        if (operationType == CacheInvalidationType.Update)
        {
            tagsToInvalidate.Add(CacheTagConstants.CustomerTag(customer.Id));
        }

        await _cacheService.InvalidateTagsAsync(tagsToInvalidate.ToArray());
    }
}