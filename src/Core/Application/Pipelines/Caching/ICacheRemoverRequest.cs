namespace Core.Application.Pipelines.Caching;

public interface ICacheRemoverRequest
{
    string CacheGroupKey { get; }
}
