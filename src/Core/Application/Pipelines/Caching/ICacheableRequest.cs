namespace Core.Application.Pipelines.Caching;

public interface ICacheableRequest
{
    string CacheKey { get; }
    string? CacheGroupKey { get; }
    TimeSpan? SlidingExpiration { get; }
}
