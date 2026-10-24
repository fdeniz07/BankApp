using Microsoft.Extensions.DependencyInjection;

namespace Application.Modules.Customers;

/// <summary>
/// Customer bounded context: individual / corporate customers and lifecycle rules.
/// </summary>
public static class CustomersModule
{
    public const string Name = "Customers";

    public static IServiceCollection AddCustomersModule(this IServiceCollection services)
    {
        return services;
    }
}
