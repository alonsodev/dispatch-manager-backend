namespace DispatchManager.Application.Services.CacheStrategies;

public static class CacheTagConstants
{
    // Tags por entidad
    public const string ORDERS = "orders";
    public const string CUSTOMERS = "customers";
    public const string PRODUCTS = "products";

    // Tags específicos por ID
    public static string OrderTag(Guid orderId) => $"order:{orderId}";
    public static string CustomerTag(Guid customerId) => $"customer:{customerId}";
    public static string ProductTag(Guid productId) => $"product:{productId}";

    // Tags por relaciones
    public static string CustomerOrdersTag(Guid customerId) => $"customer_orders:{customerId}";
    public static string ProductOrdersTag(Guid productId) => $"product_orders:{productId}";

    // Tags por consultas
    public const string ORDER_LISTS = "order_lists";
    public const string CUSTOMER_LISTS = "customer_lists";
    public const string PRODUCT_LISTS = "product_lists";
    public const string REPORTS = "reports";
}