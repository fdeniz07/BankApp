using Application.Modules.Credit.Sagas;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Modules.Credit;

/// <summary>
/// Credit bounded context: catalog, applications, dashboard, saga / process manager.
/// </summary>
public static class CreditModule
{
    public const string Name = "Credit";

    public static IServiceCollection AddCreditModule(this IServiceCollection services)
    {
        services.AddScoped<ICreditApplicationProcessManager, CreditApplicationProcessManager>();
        return services;
    }
}
