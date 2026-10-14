namespace Core.Caching;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? slidingExpiration = null,
        string? cacheGroupKey = null,
        CancellationToken cancellationToken = default);

    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    Task RemoveByGroupAsync(string cacheGroupKey, CancellationToken cancellationToken = default);
}
