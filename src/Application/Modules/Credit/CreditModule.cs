using Application.Modules.Credit.Sagas;
using Application.Services.Amortization.Export;
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

        // Payment-plan document exporters (pdf today; register csv/xlsx exporters later).
        services.AddSingleton<IPaymentPlanDocumentExporter, PaymentPlanPdfExporter>();
        services.AddSingleton<IPaymentPlanDocumentExporterResolver, PaymentPlanDocumentExporterResolver>();

        return services;
    }
}
