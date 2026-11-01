using Application.Modules.Credit.Features.CreditApplicationDocuments;
using Application.Modules.Credit.Features.CreditApplications.ApprovalAuthority;
using Application.Services.Search;
using Elastic.Clients.Elasticsearch;
using WebApi.Modules.Credit.Idempotency;
using WebApi.Modules.Credit.Search;

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

        ElasticsearchOptions es = configuration.GetSection(ElasticsearchOptions.SectionName).Get<ElasticsearchOptions>()
            ?? new ElasticsearchOptions();
        if (es.Enabled && es.Nodes is { Length: > 0 })
        {
            var settings = new ElasticsearchClientSettings(new Uri(es.Nodes[0]));
            services.AddSingleton(new ElasticsearchClient(settings));
            services.AddScoped<ICreditApplicationSearchService, ElasticCreditApplicationSearchService>();
        }
        else
        {
            services.AddSingleton<ICreditApplicationSearchService, Application.Services.Search.NullCreditApplicationSearchService>();
        }

        return services;
    }
}
