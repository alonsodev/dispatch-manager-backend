using DispatchManager.Application.Contracts.Infrastructure;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DispatchManager.Infrastructure.HealthChecks;

public sealed class CacheHealthCheck : IHealthCheck
{
    private readonly ICacheService _cacheService;

    public CacheHealthCheck(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var testKey = $"health_check_{Guid.NewGuid()}";
            var testValue = new { timestamp = DateTime.UtcNow, status = "test" };

            // Test write
            await _cacheService.SetAsync(testKey, testValue, TimeSpan.FromSeconds(30), cancellationToken);

            // Test read
            var retrievedValue = await _cacheService.GetAsync<object>(testKey, cancellationToken);

            // Clean up
            await _cacheService.RemoveAsync(testKey, cancellationToken);

            return retrievedValue != null
                ? HealthCheckResult.Healthy("MemoryCache funcionando correctamente")
                : HealthCheckResult.Degraded("MemoryCache falló prueba lectura/escritura");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("MemoryCache no accesible", ex);
        }
    }
}