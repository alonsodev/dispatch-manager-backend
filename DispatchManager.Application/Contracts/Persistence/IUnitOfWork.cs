using System.Linq.Expressions;

namespace DispatchManager.Application.Contracts.Persistence;

/// <summary>
/// Unit of Work pattern interface for Application layer
/// Defines transactional boundaries and repository access for business operations
/// </summary>
public interface IUnitOfWork : IDisposable
{
    // Repository Properties
    IOrderRepository Orders { get; }
    ICustomerRepository Customers { get; }
    IProductRepository Products { get; }

    // Transaction Management
    /// <summary>
    /// Save all pending changes to the database
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of affected records</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begin a new database transaction
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the operation</returns>
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Commit the current transaction
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the operation</returns>
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rollback the current transaction
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the operation</returns>
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

    // Bulk Operations
    /// <summary>
    /// Execute bulk update operation
    /// </summary>
    /// <typeparam name="TEntity">Entity type</typeparam>
    /// <param name="predicate">Filter predicate</param>
    /// <param name="updateExpression">Update expression</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of affected records</returns>
    Task<int> ExecuteBulkUpdateAsync<TEntity>(
        Expression<Func<TEntity, bool>> predicate,
        Expression<Func<TEntity, TEntity>> updateExpression,
        CancellationToken cancellationToken = default) where TEntity : class;

    /// <summary>
    /// Execute bulk delete operation
    /// </summary>
    /// <typeparam name="TEntity">Entity type</typeparam>
    /// <param name="predicate">Filter predicate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of affected records</returns>
    Task<int> ExecuteBulkDeleteAsync<TEntity>(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default) where TEntity : class;
}
