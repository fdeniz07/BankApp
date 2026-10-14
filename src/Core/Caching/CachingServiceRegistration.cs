using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Caching;

public static class CachingServiceRegistration
{
    public static IServiceCollection AddCoreCaching(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<JsonSerializerOptionsRegistration>? configureJson = null)
    {
        services.Configure<CacheSettings>(configuration.GetSection(CacheSettings.SectionName));

        JsonSerializerOptionsRegistration registration = new();
        configureJson?.Invoke(registration);

        services.AddSingleton<ICacheService>(sp =>
        {
            return new DistributedCacheService(
                sp.GetRequiredService<Microsoft.Extensions.Caching.Distributed.IDistributedCache>(),
                sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<CacheSettings>>(),
                sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<DistributedCacheService>>(),
                registration.Options);
        });

        return services;
    }
}

public sealed class JsonSerializerOptionsRegistration
{
    public System.Text.Json.JsonSerializerOptions Options { get; } = new()
    {
        PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
    };
}
