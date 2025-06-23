using DispatchManager.Application.Contracts.Infrastructure;

namespace DispatchManager.Application.Services.CacheStrategies;

public static class OrderCacheStrategy
{
    private const string ORDER_PREFIX = "order:";
    private const string CUSTOMER_ORDERS_PREFIX = "customer_orders:";
    private const string ROUTE_CACHE_PREFIX = "route:";

    public static string GetOrderKey(int orderId) => $"{ORDER_PREFIX}{orderId}";
    public static string GetCustomerOrdersKey(int customerId) => $"{CUSTOMER_ORDERS_PREFIX}{customerId}";
    public static string GetRouteKey(string origin, string destination) =>
        $"{ROUTE_CACHE_PREFIX}{origin}_{destination}";

    // Tiempos de expiración según criticidad
    public static readonly TimeSpan OrderCacheTime = TimeSpan.FromMinutes(0);      // Órdenes activas
    public static readonly TimeSpan RouteCacheTime = TimeSpan.FromHours(0);         // Rutas calculadas
    public static readonly TimeSpan CustomerCacheTime = TimeSpan.FromMinutes(0);   // Datos cliente
}