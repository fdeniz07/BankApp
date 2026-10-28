using Application.Services.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Contexts;
using Persistence.Repositories;

namespace Persistence;

public static class PersistenceServiceRegistration
{
    public static IServiceCollection AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<BankingDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("BankingConnectionString")));

        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IIndividualCustomerRepository, IndividualCustomerRepository>();
        services.AddScoped<ICorporateCustomerRepository, CorporateCustomerRepository>();
        services.AddScoped<ICreditTypeRepository, CreditTypeRepository>();
        services.AddScoped<ICreditSubTypeRepository, CreditSubTypeRepository>();
        services.AddScoped<ICreditApplicationRepository, CreditApplicationRepository>();
        services.AddScoped<ICreditApplicationDocumentRepository, CreditApplicationDocumentRepository>();
        services.AddScoped<ICreditApplicationSagaRepository, CreditApplicationSagaRepository>();
        services.AddScoped<IApplicationUserRepository, ApplicationUserRepository>();
        services.AddScoped<IOperationClaimRepository, OperationClaimRepository>();
        services.AddScoped<IUserOperationClaimRepository, UserOperationClaimRepository>();

        return services;
    }
}
