using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Core.Caching;

public class DistributedCacheService : ICacheService
{
    private readonly IDistributedCache _distributedCache;
    private readonly CacheSettings _cacheSettings;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly ILogger<DistributedCacheService> _logger;

    public DistributedCacheService(
        IDistributedCache distributedCache,
        IOptions<CacheSettings> cacheSettings,
        ILogger<DistributedCacheService> logger,
        JsonSerializerOptions? jsonSerializerOptions = null)
    {
        _distributedCache = distributedCache;
        _cacheSettings = cacheSettings.Value;
        _logger = logger;
        _jsonSerializerOptions = jsonSerializerOptions ?? new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        byte[]? cachedBytes = await _distributedCache.GetAsync(key, cancellationToken);
        if (cachedBytes is null || cachedBytes.Length == 0)
        {
            _logger.LogDebug("Cache miss for {CacheKey}", key);
            return default;
        }

        _logger.LogDebug("Cache hit for {CacheKey}", key);
        string json = Encoding.UTF8.GetString(cachedBytes);
        return JsonSerializer.Deserialize<T>(json, _jsonSerializerOptions);
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? slidingExpiration = null,
        string? cacheGroupKey = null,
        CancellationToken cancellationToken = default)
    {
        TimeSpan expiration = slidingExpiration
            ?? TimeSpan.FromMinutes(_cacheSettings.SlidingExpirationMinutes);

        DistributedCacheEntryOptions options = new()
        {
            SlidingExpiration = expiration
        };

        byte[] bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value, _jsonSerializerOptions));
        await _distributedCache.SetAsync(key, bytes, options, cancellationToken);
        _logger.LogDebug("Cache set for {CacheKey} (sliding {SlidingExpiration})", key, expiration);

        if (!string.IsNullOrWhiteSpace(cacheGroupKey))
            await AddKeyToGroupAsync(cacheGroupKey, key, expiration, cancellationToken);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await _distributedCache.RemoveAsync(key, cancellationToken);
        _logger.LogDebug("Cache removed for {CacheKey}", key);
    }

    public async Task RemoveByGroupAsync(string cacheGroupKey, CancellationToken cancellationToken = default)
    {
        string groupStoreKey = GetGroupStoreKey(cacheGroupKey);
        List<string>? keys = await GetAsync<List<string>>(groupStoreKey, cancellationToken);
        if (keys is null || keys.Count == 0)
        {
            await _distributedCache.RemoveAsync(groupStoreKey, cancellationToken);
            return;
        }

        foreach (string key in keys)
            await _distributedCache.RemoveAsync(key, cancellationToken);

        await _distributedCache.RemoveAsync(groupStoreKey, cancellationToken);
        _logger.LogInformation("Cache group {CacheGroupKey} cleared ({KeyCount} keys)", cacheGroupKey, keys.Count);
    }

    private async Task AddKeyToGroupAsync(
        string cacheGroupKey,
        string cacheKey,
        TimeSpan slidingExpiration,
        CancellationToken cancellationToken)
    {
        string groupStoreKey = GetGroupStoreKey(cacheGroupKey);
        List<string> keys = await GetAsync<List<string>>(groupStoreKey, cancellationToken) ?? [];

        if (!keys.Contains(cacheKey, StringComparer.Ordinal))
            keys.Add(cacheKey);

        byte[] bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(keys, _jsonSerializerOptions));
        await _distributedCache.SetAsync(
            groupStoreKey,
            bytes,
            new DistributedCacheEntryOptions { SlidingExpiration = slidingExpiration },
            cancellationToken);
    }

    private static string GetGroupStoreKey(string cacheGroupKey) => $"CacheGroup:{cacheGroupKey}";
}
