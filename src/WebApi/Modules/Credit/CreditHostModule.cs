using Application.Modules.Credit.Features.CreditApplicationDocuments;
using WebApi.Modules.Credit.Idempotency;

namespace WebApi.Modules.Credit;

/// <summary>
/// Host-side Credit module: idempotency filter for write endpoints + document UX options.
/// </summary>
public static class CreditHostModule
{
    public static IServiceCollection AddCreditHostModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<IdempotencyOptions>(configuration.GetSection(IdempotencyOptions.SectionName));
        services.Configure<CreditDocumentsOptions>(configuration.GetSection(CreditDocumentsOptions.SectionName));
        services.AddScoped<IdempotencyKeyFilter>();
        return services;
    }
}
