namespace DispatchManager.Application.Services.CacheStrategies;

public static class CacheKeyBuilder
{
    // Keys para órdenes
    public static string OrderKey(Guid orderId) => $"order:{orderId}";
    public static string OrderWithDetailsKey(Guid orderId) => $"order_details:{orderId}";
    public static string OrdersByCustomerKey(Guid customerId) => $"orders_by_customer:{customerId}";
    public static string OrdersByCustomerWithDetailsKey(Guid customerId) => $"orders_by_customer_details:{customerId}";
    public static string OrdersByStatusKey(string status) => $"orders_by_status:{status}";
    public static string OrdersByDateRangeKey(DateTime start, DateTime end) =>
        $"orders_by_date:{start:yyyyMMdd}_{end:yyyyMMdd}";
    public static string OrderListKey()
        => $"orders:all";
    public static string OrderCountKey()
        => $"orders:count";
    public static string OrderPagedKey(int pageNumber, int pageSize, int? predicateHash, int? orderByHash, bool ascending)
        => $"orders:paged:{pageNumber}:{pageSize}:{predicateHash ?? 0}:{orderByHash ?? 0}:{ascending}";

    // Keys para clientes
    public static string CustomerKey(Guid customerId) => $"customer:{customerId}";
    public static string CustomerByEmailKey(string email) => $"customer_by_email:{email.ToLowerInvariant()}";
    public static string CustomersWithOrdersKey() => "customers_with_orders";
    public static string CustomerListKey() => "customer_list";
    public static string CustomerSearchKey(string searchTerm) => $"customer_search:{searchTerm.ToLowerInvariant()}";

    // Keys para productos
    public static string ProductKey(Guid productId) => $"product:{productId}";
    public static string ProductListKey() => "product_list";
    public static string ProductSearchKey(string searchTerm) => $"product_search:{searchTerm.ToLowerInvariant()}";
    public static string ActiveProductsKey() => "active_products";
    public static string ProductsByPriceRangeKey(decimal min, decimal max) =>
        $"products_by_price:{min:0.00}_{max:0.00}";

    // Keys para reportes
    public static string OrderCountByDistanceKey() => "order_count_by_distance";
    public static string OrderCountByDistanceForCustomerKey(Guid customerId) =>
        $"order_count_by_distance_customer:{customerId}";
    public static string OrderCountByCustomerAndIntervalKey() => "order_count_by_customer_interval";
}