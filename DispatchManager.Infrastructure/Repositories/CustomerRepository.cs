using DispatchManager.Application.Contracts.Persistence;
using DispatchManager.Domain.Entities;
using DispatchManager.Infrastructure.Data;
using DispatchManager.Infrastructure.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace DispatchManager.Infrastructure.Repositories;

public sealed class CustomerRepository : Repository<Customer>, ICustomerRepository
{
    public CustomerRepository(DispatchManagerDbContext context) : base(context) { }

    public async Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(c => c.Email == email.ToLowerInvariant(), cancellationToken);
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AnyAsync(c => c.Email == email.ToLowerInvariant(), cancellationToken);
    }

    public async Task<IReadOnlyList<Customer>> GetCustomersWithOrdersAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(c => Context.Orders.Any(o => o.CustomerId == c.Id))
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Customer>> SearchByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(c => c.Name.Contains(name))
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<(Guid Id, string Name)>> GetCustomerListAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .OrderBy(c => c.Name)
            .Select(c => new ValueTuple<Guid, string>(c.Id, c.Name))
            .ToListAsync(cancellationToken);
    }
}
