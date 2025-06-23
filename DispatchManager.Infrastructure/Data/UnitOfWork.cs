using DispatchManager.Application.Contracts.Infrastructure;
using DispatchManager.Application.Contracts.Persistence;
using DispatchManager.Application.Services.CacheStrategies;
using DispatchManager.Domain.Entities;
using DispatchManager.Infrastructure.Repositories;
using DispatchManager.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;

namespace DispatchManager.Infrastructure.Data;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly DispatchManagerDbContext _context;
    private readonly ICacheService _cacheService;
    private IDbContextTransaction? _transaction;

    private IOrderRepository? _orders;
    private ICustomerRepository? _customers;
    private IProductRepository? _products;

    public UnitOfWork(DispatchManagerDbContext context, ICacheService cacheService)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _cacheService = cacheService;
    }

    // Lazy initialization of repositories
    public IOrderRepository Orders =>
        _orders ??= new OrderRepository(_context);

    public ICustomerRepository Customers =>
        _customers ??= new CustomerRepository(_context);

    public IProductRepository Products =>
        _products ??= new ProductRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Capturar entidades modificadas antes de guardar
            var modifiedEntities = CaptureModifiedEntities();

            var result = await _context.SaveChangesAsync(cancellationToken);

            // Invalidar cache después de guardar exitosamente
            await InvalidateCacheForModifiedEntities(modifiedEntities);

            return result;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new InvalidOperationException(
                "The record you attempted to edit was modified by another user after you got the original value. " +
                "Please reload the data and try again.", ex);
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException(
                "An error occurred while saving to the database. Please check your data and try again.", ex);
        }
    }

    private List<(Type EntityType, object Entity, EntityState State)> CaptureModifiedEntities()
    {
        return _context.ChangeTracker.Entries()
            .Where(e => e.State != EntityState.Unchanged)
            .Select(e => (e.Entity.GetType(), e.Entity, e.State))
            .ToList();
    }

    private async Task InvalidateCacheForModifiedEntities(List<(Type EntityType, object Entity, EntityState State)> modifiedEntities)
    {
        var tagsToInvalidate = new HashSet<string>();

        foreach (var (entityType, entity, state) in modifiedEntities)
        {
            if (entity is Order order)
            {
                tagsToInvalidate.Add(CacheTagConstants.ORDERS);
                tagsToInvalidate.Add(CacheTagConstants.ORDER_LISTS);
                tagsToInvalidate.Add(CacheTagConstants.CustomerOrdersTag(order.CustomerId));
                tagsToInvalidate.Add(CacheTagConstants.ProductOrdersTag(order.ProductId));
                tagsToInvalidate.Add(CacheTagConstants.REPORTS);

                if (state == EntityState.Modified)
                    tagsToInvalidate.Add(CacheTagConstants.OrderTag(order.Id));
            }
            else if (entity is Customer customer)
            {
                tagsToInvalidate.Add(CacheTagConstants.CUSTOMERS);
                tagsToInvalidate.Add(CacheTagConstants.CUSTOMER_LISTS);
                tagsToInvalidate.Add(CacheTagConstants.CustomerTag(customer.Id));
                tagsToInvalidate.Add(CacheTagConstants.CustomerOrdersTag(customer.Id));
            }
            else if (entity is Product product)
            {
                tagsToInvalidate.Add(CacheTagConstants.PRODUCTS);
                tagsToInvalidate.Add(CacheTagConstants.PRODUCT_LISTS);
                tagsToInvalidate.Add(CacheTagConstants.ProductTag(product.Id));
                tagsToInvalidate.Add(CacheTagConstants.REPORTS);
                tagsToInvalidate.Add(CacheTagConstants.ProductOrdersTag(product.Id));
            }
        }

        if (tagsToInvalidate.Any())
        {
            await _cacheService.InvalidateTagsAsync(tagsToInvalidate.ToArray());
        }
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            throw new InvalidOperationException("A transaction is already in progress.");
        }

        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("No transaction in progress to commit.");
        }

        try
        {
            await _transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("No transaction in progress to rollback.");
        }

        try
        {
            await _transaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task<int> ExecuteBulkUpdateAsync<TEntity>(
        Expression<Func<TEntity, bool>> predicate,
        Expression<Func<TEntity, TEntity>> updateExpression,
        CancellationToken cancellationToken = default) where TEntity : class
    {
        // For EF Core bulk operations, you might want to use EF Core Extensions
        // For now, we'll use a basic implementation
        var entities = await _context.Set<TEntity>()
            .Where(predicate)
            .ToListAsync(cancellationToken);

        if (!entities.Any())
            return 0;

        // This is a simplified implementation
        // In production, consider using libraries like EFCore.BulkExtensions
        var compiled = updateExpression.Compile();
        foreach (var entity in entities)
        {
            var updated = compiled(entity);
            _context.Entry(entity).CurrentValues.SetValues(updated);
        }

        return await SaveChangesAsync(cancellationToken);
    }

    public async Task<int> ExecuteBulkDeleteAsync<TEntity>(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default) where TEntity : class
    {
        var entities = await _context.Set<TEntity>()
            .Where(predicate)
            .ToListAsync(cancellationToken);

        if (!entities.Any())
            return 0;

        _context.Set<TEntity>().RemoveRange(entities);
        return await SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
