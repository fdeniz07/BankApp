using Core.Caching;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Core.Application.Pipelines.Caching;

public class CacheRemovingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, ICacheRemoverRequest
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<CacheRemovingBehavior<TRequest, TResponse>> _logger;

    public CacheRemovingBehavior(
        ICacheService cacheService,
        ILogger<CacheRemovingBehavior<TRequest, TResponse>> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        TResponse response = await next();

        await _cacheService.RemoveByGroupAsync(request.CacheGroupKey, cancellationToken);
        _logger.LogInformation("Cache group invalidated: {CacheGroupKey}", request.CacheGroupKey);

        return response;
    }
}
