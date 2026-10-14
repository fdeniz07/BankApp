using Core.Caching;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Core.Application.Pipelines.Caching;

public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, ICacheableRequest
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;

    public CachingBehavior(
        ICacheService cacheService,
        ILogger<CachingBehavior<TRequest, TResponse>> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        TResponse? cached = await _cacheService.GetAsync<TResponse>(request.CacheKey, cancellationToken);
        if (cached is not null)
        {
            _logger.LogInformation("Returning cached response for {CacheKey}", request.CacheKey);
            return cached;
        }

        TResponse response = await next();

        await _cacheService.SetAsync(
            request.CacheKey,
            response,
            request.SlidingExpiration,
            request.CacheGroupKey,
            cancellationToken);

        return response;
    }
}
