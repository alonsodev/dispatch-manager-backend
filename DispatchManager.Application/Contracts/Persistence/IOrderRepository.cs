using DispatchManager.Domain.Entities;
using DispatchManager.Domain.Enums;

namespace DispatchManager.Application.Contracts.Persistence;

/// <summary>
/// Repository contract for Order entity operations in Application layer
/// </summary>
public interface IOrderRepository : IRepository<Order>
{
    // Domain-specific queries
    Task<IReadOnlyList<Order>> GetOrdersByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetOrdersByCustomerIdWithDetailsAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetOrdersByStatusAsync(OrderStatus status, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Order>> GetOrdersWithDetailsAsync(CancellationToken cancellationToken = default);

    // Reporting queries
    Task<Dictionary<string, int>> GetOrderCountByDistanceIntervalAsync(CancellationToken cancellationToken = default);
    Task<Dictionary<string, int>> GetOrderCountByDistanceIntervalForCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<(Guid CustomerId, string CustomerName, string DistanceInterval, int OrderCount)>>
        GetOrderCountByCustomerAndIntervalAsync(CancellationToken cancellationToken = default);

    // Performance queries
    Task<Order?> GetOrderWithFullDetailsAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<bool> HasOrdersInProgressForCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);
}