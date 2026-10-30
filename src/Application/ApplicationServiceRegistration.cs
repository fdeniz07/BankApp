using System.Reflection;
using Application.Modules;
using Application.Services.Auth;
using Application.Services.Branches;
using Application.Services.Logging;
using Core.Application.Pipelines;
using Core.Application.Pipelines.Authorization;
using Core.Application.Pipelines.Caching;
using Core.Application.Pipelines.Logging;
using Core.Application.Pipelines.Validation;
using Core.Security.JWT;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class ApplicationServiceRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddAutoMapper(configuration =>
        {
            configuration.AddMaps(Assembly.GetExecutingAssembly());
        });
        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            configuration.AddOpenBehavior(typeof(LoggingBehavior<,>));
            configuration.AddOpenBehavior(typeof(AuthorizationBehavior<,>));
            configuration.AddOpenBehavior(typeof(RequestValidationBehavior<,>));
            configuration.AddOpenBehavior(typeof(CachingBehavior<,>));
            configuration.AddOpenBehavior(typeof(CacheRemovingBehavior<,>));
        });
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddScoped<ITokenHelper, JwtHelper>();
        services.AddScoped<IAuditLogger, SerilogAuditLogger>();
        services.AddScoped<IBranchAuthorizationService, BranchAuthorizationService>();
        services.AddScoped<ICatalogLocaleService, CatalogLocaleService>();
        services.AddSubClassesOfType(Assembly.GetExecutingAssembly(), typeof(BaseBusinessRules));
        services.AddApplicationModules();

        return services;
    }

    public static IServiceCollection AddSubClassesOfType(
        this IServiceCollection services,
        Assembly assembly,
        Type type,
        Func<IServiceCollection, Type, IServiceCollection>? addWithLifeCycle = null)
    {
        var types = assembly.GetTypes().Where(t => t.IsSubclassOf(type) && type != t).ToList();
        foreach (Type? item in types)
        {
            if (addWithLifeCycle is null)
                services.AddScoped(item);
            else
                addWithLifeCycle(services, item);
        }

        return services;
    }
}
