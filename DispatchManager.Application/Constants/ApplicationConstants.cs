namespace DispatchManager.Application.Constants;

public static class ApplicationConstants
{
    public static class Orders
    {
        public const int MaxOrdersPerQuery = 1000;
        public const int DefaultPageSize = 10;
        public const string DefaultSortField = "CreatedAt";
    }

    public static class DistanceIntervals
    {
        public const string Interval1 = "1-50 km";
        public const string Interval2 = "51-200 km";
        public const string Interval3 = "201-500 km";
        public const string Interval4 = "501-1000 km";
    }

    public static class Validation
    {
        public const int MaxCustomerNameLength = 200;
        public const int MaxProductNameLength = 200;
        public const int MaxEmailLength = 320;
        public const int MaxPhoneLength = 20;
    }

    public static class CacheKeys
    {
        public const string CustomersKey = "customers";
        public const string ProductsKey = "products";
        public const string OrdersReportKey = "orders_report";
        public const int DefaultCacheDurationMinutes = 15;
    }
}