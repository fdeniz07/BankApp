using WebApi.Modules.Credit.Idempotency;

namespace WebApi.Modules.Credit;

/// <summary>
/// Host-side Credit module: idempotency filter for write endpoints.
/// </summary>
public static class CreditHostModule
{
    public static IServiceCollection AddCreditHostModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<IdempotencyOptions>(configuration.GetSection(IdempotencyOptions.SectionName));
        services.AddScoped<IdempotencyKeyFilter>();
        return services;
    }
}
