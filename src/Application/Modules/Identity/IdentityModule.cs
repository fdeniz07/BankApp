using Microsoft.Extensions.DependencyInjection;

namespace Application.Modules.Identity;

/// <summary>
/// Identity bounded context: Auth, staff users, profiles, profile images.
/// Feature handlers are discovered via the Application assembly (MediatR / FluentValidation / AutoMapper).
/// </summary>
public static class IdentityModule
{
    public const string Name = "Identity";

    public static IServiceCollection AddIdentityModule(this IServiceCollection services)
    {
        // Handlers/validators/maps live under Modules/Identity/Features and are registered
        // assembly-wide from ApplicationServiceRegistration.
        return services;
    }
}
