using Application.Modules.Credit;
using Application.Modules.Customers;
using Application.Modules.Identity;
using Application.Modules.Notification;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Modules;

public static class ModuleRegistration
{
    /// <summary>
    /// Registers all application modules (modular monolith composition).
    /// </summary>
    public static IServiceCollection AddApplicationModules(this IServiceCollection services)
    {
        services.AddIdentityModule();
        services.AddCustomersModule();
        services.AddCreditModule();
        services.AddNotificationModule();
        return services;
    }
}
