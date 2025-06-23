using DispatchManager.Application.Contracts.Infrastructure;

namespace DispatchManager.Application.Contracts.Infrastructure;

public interface ICacheService
{
    // Métodos básicos existentes
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class;
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default);

    // Nuevos métodos para tags
    Task SetWithTagsAsync<T>(string key, T value, string[] tags, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class;
    Task InvalidateTagAsync(string tag, CancellationToken cancellationToken = default);
    Task InvalidateTagsAsync(string[] tags, CancellationToken cancellationToken = default);

    Task InvalidateAllAsync(CancellationToken cancellationToken = default);
    Task<CacheStatsResponse> GetStatsAsync(CancellationToken cancellationToken = default);
}