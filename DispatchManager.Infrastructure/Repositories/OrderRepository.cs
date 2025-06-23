using DispatchManager.Application.Contracts.Persistence;
using DispatchManager.Domain.Entities;
using DispatchManager.Domain.Enums;
using DispatchManager.Infrastructure.Data;
using DispatchManager.Infrastructure.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace DispatchManager.Infrastructure.Repositories;

public sealed class OrderRepository : Repository<Order>, IOrderRepository
{
    public OrderRepository(DispatchManagerDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Order>> GetOrdersWithDetailsAsync(
    CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(o => o.Customer)
            .Include(o => o.Product)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }
    public async Task<IReadOnlyList<Order>> GetOrdersByCustomerIdAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetOrdersByCustomerIdWithDetailsAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(o => o.Customer)
            .Include(o => o.Product)
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetOrdersByDateRangeAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetOrdersByStatusAsync(
        OrderStatus status,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(o => o.Status == status)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Dictionary<string, int>> GetOrderCountByDistanceIntervalAsync(
        CancellationToken cancellationToken = default)
    {
        var orders = await DbSet
            .Select(o => new { DistanceKm = o.Distance.Kilometers })
            .ToListAsync(cancellationToken);

        return orders
            .GroupBy(o => GetDistanceInterval(o.DistanceKm))
            .ToDictionary(g => g.Key, g => g.Count());
    }

    public async Task<Dictionary<string, int>> GetOrderCountByDistanceIntervalForCustomerAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        var orders = await DbSet
            .Where(o => o.CustomerId == customerId)
            .Select(o => new { DistanceKm = o.Distance.Kilometers })
            .ToListAsync(cancellationToken);

        return orders
            .GroupBy(o => GetDistanceInterval(o.DistanceKm))
            .ToDictionary(g => g.Key, g => g.Count());
    }

    public async Task<IReadOnlyList<(Guid CustomerId, string CustomerName, string DistanceInterval, int OrderCount)>>
        GetOrderCountByCustomerAndIntervalAsync(CancellationToken cancellationToken = default)
    {
        var orders = await DbSet
            .Include(o => o.Customer)
            .Select(o => new
            {
                CustomerId = o.CustomerId,
                CustomerName = o.Customer.Name,
                DistanceKm = o.Distance.Kilometers
            })
            .ToListAsync(cancellationToken);

        return orders
            .GroupBy(o => new { o.CustomerId, o.CustomerName })
            .SelectMany(customerGroup =>
                customerGroup
                    .GroupBy(o => GetDistanceInterval(o.DistanceKm))
                    .Select(intervalGroup => (
                        CustomerId: customerGroup.Key.CustomerId,
                        CustomerName: customerGroup.Key.CustomerName,
                        DistanceInterval: intervalGroup.Key,
                        OrderCount: intervalGroup.Count()
                    ))
            )
            .OrderBy(x => x.CustomerName)
            .ThenBy(x => x.DistanceInterval)
            .ToList();
    }

    public async Task<Order?> GetOrderWithFullDetailsAsync(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(o => o.Customer)
            .Include(o => o.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
    }

    public async Task<bool> HasOrdersInProgressForCustomerAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AnyAsync(o => o.CustomerId == customerId &&
                          (o.Status == OrderStatus.Created || o.Status == OrderStatus.InProgress),
                     cancellationToken);
    }

    private static string GetDistanceInterval(double kilometers)
    {
        return kilometers switch
        {
            >= 1 and <= 50 => "1-50 km",
            >= 51 and <= 200 => "51-200 km",
            >= 201 and <= 500 => "201-500 km",
            >= 501 and <= 1000 => "501-1000 km",
            _ => "Invalid range"
        };
    }
}
