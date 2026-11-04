using Application.Modules.Credit.Features.CreditApplicationDocuments;
using Application.Modules.Credit.Features.CreditApplications.ApprovalAuthority;
using Application.Services.Search;
using Elastic.Clients.Elasticsearch;
using WebApi.Modules.Credit.Idempotency;
using WebApi.Modules.Credit.Search;
using WebApi.Modules.Customers.Search;
using WebApi.Modules.Identity.Search;

namespace WebApi.Modules.Credit;

/// <summary>
/// Host-side Credit module: idempotency, document gate, approval authority, Elasticsearch search.
/// </summary>
public static class CreditHostModule
{
    public static IServiceCollection AddCreditHostModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<IdempotencyOptions>(configuration.GetSection(IdempotencyOptions.SectionName));
        services.Configure<CreditDocumentsOptions>(configuration.GetSection(CreditDocumentsOptions.SectionName));
        services.Configure<ApprovalAuthorityOptions>(configuration.GetSection(ApprovalAuthorityOptions.SectionName));
        services.Configure<ElasticsearchOptions>(configuration.GetSection(ElasticsearchOptions.SectionName));
        services.Configure<LogstashOptions>(configuration.GetSection(LogstashOptions.SectionName));
        services.AddScoped<IdempotencyKeyFilter>();

        IConfigurationSection esSection = configuration.GetSection(ElasticsearchOptions.SectionName);
        ElasticsearchOptions es = esSection.Get<ElasticsearchOptions>() ?? new ElasticsearchOptions();
        // Compose sets Elasticsearch__Nodes__0; JSON array bind can stick to appsettings localhost — prefer indexed key.
        string? node0 = esSection["Nodes:0"];
        if (!string.IsNullOrWhiteSpace(node0))
            es.Nodes = [node0.Trim()];

        if (es.Enabled && es.Nodes is { Length: > 0 })
        {
            string node = es.Nodes[0];
            var settings = new ElasticsearchClientSettings(new Uri(node));
            services.AddSingleton(new ElasticsearchClient(settings));
            services.AddScoped<ICreditApplicationSearchService, ElasticCreditApplicationSearchService>();
            services.AddScoped<IStaffUserSearchService, ElasticStaffUserSearchService>();
            services.AddScoped<ICustomerSearchService, ElasticCustomerSearchService>();
            services.AddScoped<ICreditTypeSearchService, ElasticCreditTypeSearchService>();
        }
        else
        {
            services.AddSingleton<ICreditApplicationSearchService, Application.Services.Search.NullCreditApplicationSearchService>();
            services.AddSingleton<IStaffUserSearchService, NullStaffUserSearchService>();
            services.AddSingleton<ICustomerSearchService, NullCustomerSearchService>();
            services.AddSingleton<ICreditTypeSearchService, NullCreditTypeSearchService>();
        }

        return services;
    }
}
