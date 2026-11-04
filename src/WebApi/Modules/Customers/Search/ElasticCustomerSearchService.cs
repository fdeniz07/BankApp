using Application.Modules.Credit.Features.CreditApplications.Queries.GetFacets;
using Application.Services.Repositories;
using Application.Services.Search;
using Domain.Entities;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Aggregations;
using Elastic.Clients.Elasticsearch.Mapping;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace WebApi.Modules.Customers.Search;

public sealed class ElasticCustomerSearchService : ICustomerSearchService
{
    private readonly ElasticsearchClient _client;
    private readonly ElasticsearchOptions _options;
    private readonly IIndividualCustomerRepository _individuals;
    private readonly ICorporateCustomerRepository _corporates;
    private readonly IBranchRepository _branches;
    private readonly ILogger<ElasticCustomerSearchService> _logger;

    public ElasticCustomerSearchService(
        ElasticsearchClient client,
        IOptions<ElasticsearchOptions> options,
        IIndividualCustomerRepository individuals,
        ICorporateCustomerRepository corporates,
        IBranchRepository branches,
        ILogger<ElasticCustomerSearchService> logger)
    {
        _client = client;
        _options = options.Value;
        _individuals = individuals;
        _corporates = corporates;
        _branches = branches;
        _logger = logger;
    }

    public bool IsAvailable => _options.Enabled;

    private string IndexName => _options.CustomersIndex;

    public async Task EnsureIndexAsync(CancellationToken cancellationToken = default)
    {
        if (!IsAvailable)
            return;

        var exists = await _client.Indices.ExistsAsync(IndexName, cancellationToken);
        if (exists.Exists)
            return;

        var created = await _client.Indices.CreateAsync(IndexName, c => c
            .Mappings(m => m
                .Properties(new Properties
                {
                    { "id", new KeywordProperty() },
                    { "kind", new KeywordProperty() },
                    { "customerNumber", new TextProperty { Fields = new Properties { { "keyword", new KeywordProperty() } } } },
                    { "email", new TextProperty { Fields = new Properties { { "keyword", new KeywordProperty() } } } },
                    { "phoneNumber", new TextProperty { Fields = new Properties { { "keyword", new KeywordProperty() } } } },
                    { "firstName", new TextProperty() },
                    { "lastName", new TextProperty() },
                    { "nationalId", new KeywordProperty() },
                    { "companyName", new TextProperty() },
                    { "taxNumber", new KeywordProperty() },
                    { "authorizedPersonName", new TextProperty() },
                    { "branchId", new KeywordProperty() },
                    { "branchCode", new KeywordProperty() },
                    { "branchName", new TextProperty { Fields = new Properties { { "keyword", new KeywordProperty() } } } },
                    { "isActive", new BooleanProperty() },
                    { "isDeleted", new BooleanProperty() }
                })), cancellationToken);

        if (!created.IsValidResponse)
            _logger.LogWarning("Elasticsearch customers index create issue: {Error}", created.DebugInformation);
    }

    public async Task IndexAsync(CustomerSearchDocument document, CancellationToken cancellationToken = default)
    {
        if (!IsAvailable)
            return;

        try
        {
            await EnsureIndexAsync(cancellationToken);
            await _client.IndexAsync(document, i => i.Index(IndexName).Id(document.Id.ToString()), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to index customer {Id}", document.Id);
        }
    }

    public async Task UpsertFromIndividualAsync(IndividualCustomer customer, CancellationToken cancellationToken = default)
    {
        if (!IsAvailable)
            return;

        await IndexAsync(await BuildIndividualAsync(customer, cancellationToken), cancellationToken);
    }

    public async Task UpsertFromCorporateAsync(CorporateCustomer customer, CancellationToken cancellationToken = default)
    {
        if (!IsAvailable)
            return;

        await IndexAsync(await BuildCorporateAsync(customer, cancellationToken), cancellationToken);
    }

    public async Task DeleteAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        if (!IsAvailable)
            return;

        try
        {
            await _client.DeleteAsync(new DeleteRequest(IndexName, customerId.ToString()), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete customer {Id} from index", customerId);
        }
    }

    public async Task ReindexAllAsync(CancellationToken cancellationToken = default)
    {
        if (!IsAvailable)
            return;

        await EnsureIndexAsync(cancellationToken);

        var individuals = await _individuals.GetListAsync(
            include: q => q.Include(c => c.Branch),
            withDeleted: true,
            index: 0,
            size: 500,
            cancellationToken: cancellationToken);
        foreach (IndividualCustomer c in individuals.Items)
            await UpsertFromIndividualAsync(c, cancellationToken);

        var corporates = await _corporates.GetListAsync(
            include: q => q.Include(c => c.Branch),
            withDeleted: true,
            index: 0,
            size: 500,
            cancellationToken: cancellationToken);
        foreach (CorporateCustomer c in corporates.Items)
            await UpsertFromCorporateAsync(c, cancellationToken);

        _logger.LogInformation(
            "Reindexed {Individuals} individual + {Corporates} corporate customers into {Index}",
            individuals.Items.Count,
            corporates.Items.Count,
            IndexName);
    }

    public async Task<CustomerSearchPage?> SearchAsync(
        CustomerSearchQuery query,
        CancellationToken cancellationToken = default)
    {
        if (!IsAvailable)
            return null;

        bool hasText = !string.IsNullOrWhiteSpace(query.Text);
        bool hasFilters = query.BranchId.HasValue
            || query.RequiredBranchScope.HasValue
            || query.IsActive.HasValue
            || query.OnlyDeleted;

        if (!hasText && !hasFilters)
            return null;

        try
        {
            await EnsureIndexAsync(cancellationToken);
            int from = Math.Max(query.PageIndex, 0) * Math.Max(query.PageSize, 1);
            BoolQuery boolQuery = BuildBoolQuery(query);

            SearchResponse<CustomerSearchDocument> response =
                await _client.SearchAsync<CustomerSearchDocument>(s =>
                {
                    s.Index(IndexName).From(from).Size(query.PageSize);
                    s.Query(q => q.Bool(boolQuery));
                    s.Sort(new[]
                    {
                        SortOptions.Field(
                            new Field("customerNumber.keyword"),
                            new FieldSort { Order = SortOrder.Asc })
                    });
                }, cancellationToken);

            if (!response.IsValidResponse)
            {
                _logger.LogWarning("Elasticsearch customer search failed: {Info}", response.DebugInformation);
                return null;
            }

            long total = response.Total;
            int size = Math.Max(query.PageSize, 1);
            return new CustomerSearchPage
            {
                Items = response.Documents.ToList(),
                Index = query.PageIndex,
                Size = size,
                Count = (int)Math.Min(total, int.MaxValue),
                Pages = size == 0 ? 0 : (int)Math.Ceiling(total / (double)size)
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Elasticsearch customer search unavailable; SQL fallback");
            return null;
        }
    }

    public async Task<CustomerFacetsResponse?> GetFacetsAsync(
        CustomerSearchQuery query,
        CancellationToken cancellationToken = default)
    {
        if (!IsAvailable)
            return null;

        try
        {
            await EnsureIndexAsync(cancellationToken);
            BoolQuery boolQuery = BuildBoolQuery(query);

            SearchResponse<CustomerSearchDocument> response =
                await _client.SearchAsync<CustomerSearchDocument>(s =>
                {
                    s.Index(IndexName).Size(0);
                    s.Query(q => q.Bool(boolQuery));
                    s.Aggregations(a =>
                    {
                        a.Add("branches", agg => agg.Terms(t => t.Field("branchId").Size(50)));
                        return a;
                    });
                }, cancellationToken);

            if (!response.IsValidResponse || response.Aggregations is null)
            {
                _logger.LogWarning("Elasticsearch customer facets failed: {Info}", response.DebugInformation);
                return null;
            }

            CustomerFacetsResponse result = new() { Source = "Elasticsearch" };
            if (response.Aggregations.TryGetValue("branches", out IAggregate? branchAgg)
                && branchAgg is StringTermsAggregate branchTerms)
            {
                result.Branches = branchTerms.Buckets
                    .Select(b => new FacetBucketDto { Key = b.Key.ToString(), Count = b.DocCount })
                    .ToList();
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Elasticsearch customer facets unavailable; SQL fallback");
            return null;
        }
    }

    private async Task<CustomerSearchDocument> BuildIndividualAsync(
        IndividualCustomer customer,
        CancellationToken cancellationToken)
    {
        (string? code, string? name) = await ResolveBranchAsync(customer, cancellationToken);
        return new CustomerSearchDocument
        {
            Id = customer.Id,
            Kind = CustomerSearchKinds.Individual,
            CustomerNumber = customer.CustomerNumber,
            Email = customer.Email,
            PhoneNumber = customer.PhoneNumber,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            NationalId = customer.NationalId,
            BranchId = customer.BranchId?.ToString(),
            BranchCode = code,
            BranchName = name,
            IsActive = customer.IsActive,
            IsDeleted = customer.IsDeleted
        };
    }

    private async Task<CustomerSearchDocument> BuildCorporateAsync(
        CorporateCustomer customer,
        CancellationToken cancellationToken)
    {
        (string? code, string? name) = await ResolveBranchAsync(customer, cancellationToken);
        return new CustomerSearchDocument
        {
            Id = customer.Id,
            Kind = CustomerSearchKinds.Corporate,
            CustomerNumber = customer.CustomerNumber,
            Email = customer.Email,
            PhoneNumber = customer.PhoneNumber,
            CompanyName = customer.CompanyName,
            TaxNumber = customer.TaxNumber,
            AuthorizedPersonName = customer.AuthorizedPersonName,
            BranchId = customer.BranchId?.ToString(),
            BranchCode = code,
            BranchName = name,
            IsActive = customer.IsActive,
            IsDeleted = customer.IsDeleted
        };
    }

    private async Task<(string? Code, string? Name)> ResolveBranchAsync(
        Customer customer,
        CancellationToken cancellationToken)
    {
        string? code = customer.Branch?.Code;
        string? name = customer.Branch?.Name;
        if (customer.BranchId.HasValue && (code is null || name is null))
        {
            Branch? branch = await _branches.GetAsync(
                b => b.Id == customer.BranchId.Value,
                cancellationToken: cancellationToken);
            code ??= branch?.Code;
            name ??= branch?.Name;
        }

        return (code, name);
    }

    private static BoolQuery BuildBoolQuery(CustomerSearchQuery query)
    {
        List<Query> must = [];
        List<Query> filter =
        [
            new TermQuery(new Field("kind")) { Value = query.Kind }
        ];

        if (!string.IsNullOrWhiteSpace(query.Text))
        {
            string text = query.Text.Trim();
            must.Add(new MultiMatchQuery
            {
                Query = text,
                Fields = new[]
                {
                    "customerNumber^3",
                    "email^2",
                    "phoneNumber",
                    "firstName",
                    "lastName",
                    "nationalId^2",
                    "companyName^2",
                    "taxNumber^2",
                    "authorizedPersonName",
                    "branchCode",
                    "branchName"
                },
                Fuzziness = new Fuzziness("AUTO")
            });
        }

        string? branchScope = query.RequiredBranchScope?.ToString()
            ?? (query.AllowCrossBranchFilters ? query.BranchId?.ToString() : null);
        if (!string.IsNullOrEmpty(branchScope))
            filter.Add(new TermQuery(new Field("branchId")) { Value = branchScope });

        if (query.OnlyDeleted)
            filter.Add(new TermQuery(new Field("isDeleted")) { Value = true });
        else
        {
            filter.Add(new TermQuery(new Field("isDeleted")) { Value = false });
            if (query.IsActive.HasValue)
                filter.Add(new TermQuery(new Field("isActive")) { Value = query.IsActive.Value });
        }

        return new BoolQuery
        {
            Must = must.Count > 0 ? must : null,
            Filter = filter
        };
    }
}
