using DispatchManager.Application.Contracts.Infrastructure;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;

namespace DispatchManager.Infrastructure.Services;

public sealed class CacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<CacheService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    private readonly ConcurrentDictionary<string, ConcurrentHashSet<string>> _tagToKeys = new();
    private readonly ConcurrentDictionary<string, ConcurrentHashSet<string>> _keyToTags = new();

    public CacheService(
        IMemoryCache memoryCache,
        ILogger<CacheService> logger)
    {
        _memoryCache = memoryCache;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        if (string.IsNullOrWhiteSpace(key))
            return null;

        try
        {
            if (_memoryCache.TryGetValue(key, out var cachedValue) && cachedValue is T result)
            {
                _logger.LogDebug("Cache hit for key: {Key}", key);
                return result;
            }

            _logger.LogDebug("Cache miss for key: {Key}", key);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cache key: {Key}", key);
            return null;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        await SetWithTagsAsync(key, value, Array.Empty<string>(), expiration, cancellationToken);
    }

    public async Task SetWithTagsAsync<T>(string key, T value, string[] tags, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        if (string.IsNullOrWhiteSpace(key) || value == null)
            return;

        var expiry = expiration ?? TimeSpan.FromMinutes(30);

        try
        {
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiry,
                SlidingExpiration = TimeSpan.FromMinutes(5),
                Priority = CacheItemPriority.Normal,
                Size = 1
            };

            // Callback cuando se remueve del cache
            options.RegisterPostEvictionCallback((cacheKey, cacheValue, reason, state) =>
            {
                CleanupTagMappings(cacheKey.ToString());
            });

            _memoryCache.Set(key, value, options);

            // Actualizar mapeo de tags
            UpdateTagMappings(key, tags);

            _logger.LogDebug("Cache set for key: {Key}, tags: [{Tags}], expiry: {Expiry}",
                key, string.Join(", ", tags), expiry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cache key: {Key}", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
            return;

        try
        {
            _memoryCache.Remove(key);
            CleanupTagMappings(key);
            _logger.LogDebug("Cache removed for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache key: {Key}", key);
        }
    }

    public async Task InvalidateTagAsync(string tag, CancellationToken cancellationToken = default)
    {
        await InvalidateTagsAsync(new[] { tag }, cancellationToken);
    }

    public async Task InvalidateTagsAsync(string[] tags, CancellationToken cancellationToken = default)
    {
        if (tags == null || tags.Length == 0)
            return;

        try
        {
            var keysToRemove = new HashSet<string>();

            foreach (var tag in tags)
            {
                if (_tagToKeys.TryGetValue(tag, out var keys))
                {
                    foreach (var key in keys.ToArray())
                    {
                        keysToRemove.Add(key);
                    }
                }
            }

            foreach (var key in keysToRemove)
            {
                _memoryCache.Remove(key);
                CleanupTagMappings(key);
            }

            _logger.LogInformation("Invalidated {Count} cache entries for tags: [{Tags}]",
                keysToRemove.Count, string.Join(", ", tags));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating cache tags: [{Tags}]", string.Join(", ", tags));
        }
    }

    public async Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(pattern))
            return;

        try
        {
            // Para MemoryCache, implementamos con reflection (uso cuidadoso)
            var field = typeof(MemoryCache).GetField("_coherentState",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (field?.GetValue(_memoryCache) is IDictionary coherentState)
            {
                var keysToRemove = new List<object>();

                foreach (DictionaryEntry entry in coherentState)
                {
                    if (entry.Key.ToString()?.Contains(pattern, StringComparison.OrdinalIgnoreCase) == true)
                    {
                        keysToRemove.Add(entry.Key);
                    }
                }

                foreach (var key in keysToRemove)
                {
                    _memoryCache.Remove(key);
                }

                _logger.LogDebug("Removed {Count} cache entries matching pattern: {Pattern}",
                    keysToRemove.Count, pattern);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error removing cache by pattern: {Pattern}. Fallback no disponible.", pattern);
        }
    }

    private void UpdateTagMappings(string key, string[] tags)
    {
        // Limpiar mappings existentes para esta key
        CleanupTagMappings(key);

        // Crear nuevos mappings
        _keyToTags[key] = new ConcurrentHashSet<string>(tags);

        foreach (var tag in tags)
        {
            _tagToKeys.AddOrUpdate(tag,
                new ConcurrentHashSet<string> { key },
                (_, existingKeys) =>
                {
                    existingKeys.Add(key);
                    return existingKeys;
                });
        }
    }

    private void CleanupTagMappings(string key)
    {
        if (_keyToTags.TryRemove(key, out var tags))
        {
            foreach (var tag in tags)
            {
                if (_tagToKeys.TryGetValue(tag, out var keys))
                {
                    keys.Remove(key);
                    if (keys.Count == 0)
                    {
                        _tagToKeys.TryRemove(tag, out _);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Invalida toda la caché del sistema
    /// Método diseñado para ser llamado desde el controller
    /// </summary>
    public async Task InvalidateAllAsync(CancellationToken cancellationToken = default)
    {
        ((MemoryCache)_memoryCache).Compact(1.0);
        _tagToKeys.Clear();
        _keyToTags.Clear();
    }

    /// <summary>
    /// Obtiene estadísticas actuales de la caché
    /// </summary>
    public async Task<CacheStatsResponse> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var allKeys = GetAllCacheKeys();
            var totalMemory = EstimateMemoryUsage();

            var tagStats = _tagToKeys.Select(kvp => new CacheTagInfo
            {
                Tag = kvp.Key,
                KeyCount = kvp.Value.Count,
                LastAccessed = DateTime.UtcNow // Simplificado, podrías trackear esto más detalladamente
            }).ToList();

            return new CacheStatsResponse
            {
                TotalKeys = allKeys.Count,
                TotalMemoryUsage = FormatBytes(totalMemory),
                HitRate = 0.0, // Requiere implementación de métricas más avanzadas
                MissRate = 0.0, // Requiere implementación de métricas más avanzadas
                Tags = tagStats
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener estadísticas de caché");
            throw;
        }
    }

    /// <summary>
    /// Obtiene todas las claves del caché usando reflexión
    /// </summary>
    private List<string> GetAllCacheKeys()
    {
        var keys = new List<string>();

        try
        {
            // Usar reflexión para acceder al campo interno de MemoryCache
            var field = typeof(MemoryCache).GetField("_coherentState",
                BindingFlags.NonPublic | BindingFlags.Instance);

            if (field?.GetValue(_memoryCache) is IDictionary coherentState)
            {
                var dictField = coherentState.GetType().GetProperty("Store",
                    BindingFlags.NonPublic | BindingFlags.Instance);

                if (dictField?.GetValue(coherentState) is IDictionary store)
                {
                    foreach (DictionaryEntry entry in store)
                    {
                        keys.Add(entry.Key.ToString() ?? string.Empty);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "No se pudo obtener las claves del caché usando reflexión");

            // Fallback: usar las claves trackeadas por tags
            var trackedKeys = _keyToTags.Keys.ToList();
            keys.AddRange(trackedKeys);
        }

        return keys.Where(k => !string.IsNullOrEmpty(k)).Distinct().ToList();
    }

    /// <summary>
    /// Estima el uso de memoria del caché
    /// </summary>
    private long EstimateMemoryUsage()
    {
        try
        {
            // Estimación simple basada en el número de claves
            // En una implementación más robusta, podrías calcular el tamaño real de los objetos
            var keyCount = GetAllCacheKeys().Count;
            return keyCount * 1024; // Estimación de 1KB por entrada
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// Formatea bytes a una representación legible
    /// </summary>
    private static string FormatBytes(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
        double formattedBytes = bytes;
        int suffixIndex = 0;

        while (formattedBytes >= 1024 && suffixIndex < suffixes.Length - 1)
        {
            formattedBytes /= 1024;
            suffixIndex++;
        }

        return $"{formattedBytes:N2} {suffixes[suffixIndex]}";
    }
}

public class ConcurrentHashSet<T> : IDisposable, IEnumerable<T>
{
    private readonly HashSet<T> _hashSet;
    private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

    public ConcurrentHashSet()
    {
        _hashSet = new HashSet<T>();
    }

    public ConcurrentHashSet(IEnumerable<T> collection)
    {
        _hashSet = new HashSet<T>(collection);
    }

    public void Add(T item)
    {
        _lock.EnterWriteLock();
        try
        {
            _hashSet.Add(item);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public bool Remove(T item)
    {
        _lock.EnterWriteLock();
        try
        {
            return _hashSet.Remove(item);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    public bool Contains(T item)
    {
        _lock.EnterReadLock();
        try
        {
            return _hashSet.Contains(item);
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    public int Count
    {
        get
        {
            _lock.EnterReadLock();
            try
            {
                return _hashSet.Count;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }

    public T[] ToArray()
    {
        _lock.EnterReadLock();
        try
        {
            return _hashSet.ToArray();
        }
        finally
        {
            _lock.ExitReadLock();
        }
    }

    // Implementación de IEnumerable<T>
    public IEnumerator<T> GetEnumerator()
    {
        T[] items;
        _lock.EnterReadLock();
        try
        {
            items = _hashSet.ToArray();
        }
        finally
        {
            _lock.ExitReadLock();
        }

        return ((IEnumerable<T>)items).GetEnumerator();
    }

    // Implementación de IEnumerable
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Dispose()
    {
        _lock?.Dispose();
    }
}