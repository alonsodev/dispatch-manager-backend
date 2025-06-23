using DispatchManager.Domain.Entities;

namespace DispatchManager.Application.Contracts.Persistence;

/// <summary>
/// Repository contract for Customer entity operations in Application layer
/// </summary>
public interface ICustomerRepository : IRepository<Customer>
{
    // Unique constraints validation
    Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);

    // Business queries
    Task<IReadOnlyList<Customer>> GetCustomersWithOrdersAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Customer>> SearchByNameAsync(string name, CancellationToken cancellationToken = default);

    // UI support queries
    Task<IReadOnlyList<(Guid Id, string Name)>> GetCustomerListAsync(CancellationToken cancellationToken = default);
}
