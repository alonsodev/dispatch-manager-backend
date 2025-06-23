using DispatchManager.Application.Contracts.Infrastructure;
using DispatchManager.Application.Contracts.Persistence;
using DispatchManager.Infrastructure.Data;
using DispatchManager.Infrastructure.Repositories;
using DispatchManager.Infrastructure.Repositories.Base;
using DispatchManager.Infrastructure.Repositories.Decorators;
using DispatchManager.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DispatchManager.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        services.AddDbContext<DispatchManagerDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            options.UseSqlServer(connectionString, b =>
            {
                b.MigrationsAssembly(typeof(DispatchManagerDbContext).Assembly.FullName);
                b.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
            });

#if DEBUG
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
#endif
        });

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<Domain.Services.IDistanceCalculationService,
                      Domain.Services.DistanceCalculationService>();

        services.AddScoped<Domain.Services.ICostCalculationService,
                          Domain.Services.CostCalculationService>();

        // Repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();

        // Caching
        AddCachingServices(services, configuration);

        // Infrastructure Services
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IReportService, ReportService>();

        // Health Checks
        services.AddHealthChecks()
            .AddDbContextCheck<DispatchManagerDbContext>();

        return services;
    }

    private static void AddCachingServices(IServiceCollection services, IConfiguration configuration)
    {
        // Memory Cache optimizado para aplicación de despacho
        services.AddMemoryCache(options =>
        {
            options.SizeLimit = 1000; // Máximo 1000 entradas
            options.CompactionPercentage = 0.25; // Limpiar 25% cuando se alcanza el límite
            options.ExpirationScanFrequency = TimeSpan.FromSeconds(1); // Escanear cada 2 min
        });

        services.AddScoped<ICacheService, CacheService>();
    }


    public static async Task InitializeDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DispatchManagerDbContext>();

        try
        {
            // Create database if it doesn't exist
            await context.Database.EnsureCreatedAsync();

            // Apply pending migrations
            if ((await context.Database.GetPendingMigrationsAsync()).Any())
            {
                await context.Database.MigrateAsync();
            }

            // Seed initial data
            await SeedDataAsync(context);
        }
        catch (Exception ex)
        {
            // Log the exception (you might want to use ILogger here)
            throw new InvalidOperationException("An error occurred while initializing the database.", ex);
        }
    }

    private static void RegisterRepositoriesWithCache(IServiceCollection services)
    {
        // Repositories base
        services.AddScoped<OrderRepository>();
        services.AddScoped<CustomerRepository>();
        services.AddScoped<ProductRepository>();

        // Decoradores con cache
        services.AddScoped<IOrderRepository>(provider =>
        {
            var baseRepo = provider.GetRequiredService<OrderRepository>();
            var cacheService = provider.GetRequiredService<ICacheService>();
            return new CachedOrderRepository(baseRepo, cacheService);
        });

        services.AddScoped<ICustomerRepository>(provider =>
        {
            var baseRepo = provider.GetRequiredService<CustomerRepository>();
            var cacheService = provider.GetRequiredService<ICacheService>();
            return new CachedCustomerRepository(baseRepo, cacheService);
        });

        services.AddScoped<IProductRepository>(provider =>
        {
            var baseRepo = provider.GetRequiredService<ProductRepository>();
            var cacheService = provider.GetRequiredService<ICacheService>();
            return new CachedProductRepository(baseRepo, cacheService);
        });
    }

    private static async Task SeedDataAsync(DispatchManagerDbContext context)
    {
        if (await context.Customers.AnyAsync() || await context.Products.AnyAsync())
            return;

        var customers = new[]
        {
            Domain.Entities.Customer.Create("Juan Pérez", "juan.perez@email.com", "+51-999-123-456"),
            Domain.Entities.Customer.Create("María García", "maria.garcia@email.com", "+51-999-234-567"),
            Domain.Entities.Customer.Create("Carlos López", "carlos.lopez@email.com", "+51-999-345-678")
        };

        await context.Customers.AddRangeAsync(customers);

        var products = new[]
        {
            Domain.Entities.Product.Create("Laptop Dell", "Laptop Dell Inspiron 15", 2500.00m, "units"),
            Domain.Entities.Product.Create("Smartphone", "iPhone 14 Pro", 3500.00m, "units"),
            Domain.Entities.Product.Create("Arroz", "Arroz extra calidad", 5.50m, "kg"),
            Domain.Entities.Product.Create("Aceite", "Aceite vegetal premium", 8.75m, "liters")
        };

        await context.Products.AddRangeAsync(products);

        await context.SaveChangesAsync();
    }
}