using Application.Modules.Credit.Features.CreditApplications.Queries.GetFacets;
using Application.Modules.Credit.Features.CreditTypes.Queries.GetList;
using Application.Services.Repositories;
using Application.Services.Search;
using Core.Application.Responses;
using Domain.Entities;
using Domain.Enums;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Aggregations;
using Elastic.Clients.Elasticsearch.Mapping;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Microsoft.Extensions.Options;

namespace WebApi.Modules.Credit.Search;

public sealed class ElasticCreditTypeSearchService : ICreditTypeSearchService
{
    private readonly ElasticsearchClient _client;
    private readonly ElasticsearchOptions _options;
    private readonly ICreditTypeRepository _creditTypes;
    private readonly ILogger<ElasticCreditTypeSearchService> _logger;

    public ElasticCreditTypeSearchService(
        ElasticsearchClient client,
        IOptions<ElasticsearchOptions> options,
        ICreditTypeRepository creditTypes,
        ILogger<ElasticCreditTypeSearchService> logger)
    {
        _client = client;
        _options = options.Value;
        _creditTypes = creditTypes;
        _logger = logger;
    }

    public bool IsAvailable => _options.Enabled;

    private string IndexName => _options.CreditTypesIndex;

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
                    { "name", new TextProperty { Fields = new Properties { { "keyword", new KeywordProperty() } } } },
                    { "code", new TextProperty { Fields = new Properties { { "keyword", new KeywordProperty() } } } },
                    { "locale", new KeywordProperty() },
                    { "category", new KeywordProperty() },
                    { "description", new TextProperty() },
                    { "currencyCode", new KeywordProperty() },
                    { "riskLevel", new KeywordProperty() },
                    { "requiresCollateral", new BooleanProperty() },
                    { "requiresGuarantor", new BooleanProperty() },
                    { "isActive", new BooleanProperty() }
                })), cancellationToken);

        if (!created.IsValidResponse)
            _logger.LogWarning("Elasticsearch credit-types index create issue: {Error}", created.DebugInformation);
    }

    public async Task IndexAsync(CreditTypeSearchDocument document, CancellationToken cancellationToken = default)
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
            _logger.LogWarning(ex, "Failed to index credit type {Id}", document.Id);
        }
    }

    public Task UpsertFromCreditTypeAsync(CreditType creditType, CancellationToken cancellationToken = default) =>
        IndexAsync(ToDocument(creditType), cancellationToken);

    public async Task DeleteAsync(Guid creditTypeId, CancellationToken cancellationToken = default)
    {
        if (!IsAvailable)
            return;

        try
        {
            await _client.DeleteAsync(new DeleteRequest(IndexName, creditTypeId.ToString()), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete credit type {Id} from index", creditTypeId);
        }
    }

    public async Task ReindexAllAsync(CancellationToken cancellationToken = default)
    {
        if (!IsAvailable)
            return;

        await EnsureIndexAsync(cancellationToken);
        var page = await _creditTypes.GetListAsync(index: 0, size: 500, cancellationToken: cancellationToken);
        foreach (CreditType type in page.Items)
            await UpsertFromCreditTypeAsync(type, cancellationToken);

        _logger.LogInformation("Reindexed {Count} credit types into {Index}", page.Items.Count, IndexName);
    }

    public async Task<GetListResponse<GetListCreditTypeListItemDto>?> SearchAsync(
        CreditTypeSearchQuery query,
        CancellationToken cancellationToken = default)
    {
        if (!IsAvailable)
            return null;

        bool hasText = !string.IsNullOrWhiteSpace(query.Text);
        bool hasFilters = !string.IsNullOrWhiteSpace(query.Locale)
            || !string.IsNullOrWhiteSpace(query.Category)
            || !string.IsNullOrWhiteSpace(query.CurrencyCode)
            || !string.IsNullOrWhiteSpace(query.RiskLevel)
            || query.IsActive.HasValue;

        if (!hasText && !hasFilters)
            return null;

        try
        {
            await EnsureIndexAsync(cancellationToken);
            int from = Math.Max(query.PageIndex, 0) * Math.Max(query.PageSize, 1);
            BoolQuery boolQuery = BuildBoolQuery(query);

            SearchResponse<CreditTypeSearchDocument> response =
                await _client.SearchAsync<CreditTypeSearchDocument>(s =>
                {
                    s.Index(IndexName).From(from).Size(query.PageSize);
                    s.Query(q => q.Bool(boolQuery));
                    s.Sort(BuildSort(query));
                }, cancellationToken);

            if (!response.IsValidResponse)
            {
                _logger.LogWarning("Elasticsearch credit-type search failed: {Info}", response.DebugInformation);
                return null;
            }

            long total = response.Total;
            int size = Math.Max(query.PageSize, 1);
            return new GetListResponse<GetListCreditTypeListItemDto>
            {
                Items = response.Documents.Select(MapDto).ToList(),
                Index = query.PageIndex,
                Size = size,
                Count = (int)Math.Min(total, int.MaxValue),
                Pages = size == 0 ? 0 : (int)Math.Ceiling(total / (double)size),
                From = 0
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Elasticsearch credit-type search unavailable; SQL fallback");
            return null;
        }
    }

    public async Task<CreditTypeFacetsResponse?> GetFacetsAsync(
        CreditTypeSearchQuery query,
        CancellationToken cancellationToken = default)
    {
        if (!IsAvailable)
            return null;

        try
        {
            await EnsureIndexAsync(cancellationToken);
            BoolQuery boolQuery = BuildBoolQuery(query);

            SearchResponse<CreditTypeSearchDocument> response =
                await _client.SearchAsync<CreditTypeSearchDocument>(s =>
                {
                    s.Index(IndexName).Size(0);
                    s.Query(q => q.Bool(boolQuery));
                    s.Aggregations(a =>
                    {
                        a.Add("locales", agg => agg.Terms(t => t.Field("locale").Size(10)));
                        a.Add("categories", agg => agg.Terms(t => t.Field("category").Size(10)));
                        a.Add("currencies", agg => agg.Terms(t => t.Field("currencyCode").Size(20)));
                        a.Add("riskLevels", agg => agg.Terms(t => t.Field("riskLevel").Size(10)));
                        return a;
                    });
                }, cancellationToken);

            if (!response.IsValidResponse || response.Aggregations is null)
            {
                _logger.LogWarning("Elasticsearch credit-type facets failed: {Info}", response.DebugInformation);
                return null;
            }

            CreditTypeFacetsResponse result = new() { Source = "Elasticsearch" };
            result.Locales = ReadTerms(response.Aggregations, "locales");
            result.Categories = ReadTerms(response.Aggregations, "categories");
            result.Currencies = ReadTerms(response.Aggregations, "currencies");
            result.RiskLevels = ReadTerms(response.Aggregations, "riskLevels");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Elasticsearch credit-type facets unavailable; SQL fallback");
            return null;
        }
    }

    private static List<FacetBucketDto> ReadTerms(IReadOnlyDictionary<string, IAggregate> aggs, string name)
    {
        if (!aggs.TryGetValue(name, out IAggregate? agg) || agg is not StringTermsAggregate terms)
            return [];

        return terms.Buckets
            .Select(b => new FacetBucketDto { Key = b.Key.ToString(), Count = b.DocCount })
            .ToList();
    }

    private static BoolQuery BuildBoolQuery(CreditTypeSearchQuery query)
    {
        List<Query> must = [];
        List<Query> filter = [];

        if (!string.IsNullOrWhiteSpace(query.Text))
        {
            must.Add(new MultiMatchQuery
            {
                Query = query.Text.Trim(),
                Fields = new[] { "name^3", "code^3", "description", "locale", "category", "currencyCode" },
                Fuzziness = new Fuzziness("AUTO")
            });
        }

        if (!string.IsNullOrWhiteSpace(query.Locale))
            filter.Add(new TermQuery(new Field("locale")) { Value = query.Locale.Trim().ToLowerInvariant() });
        if (!string.IsNullOrWhiteSpace(query.Category))
            filter.Add(new TermQuery(new Field("category")) { Value = query.Category.Trim() });
        if (!string.IsNullOrWhiteSpace(query.CurrencyCode))
            filter.Add(new TermQuery(new Field("currencyCode")) { Value = query.CurrencyCode.Trim().ToUpperInvariant() });
        if (!string.IsNullOrWhiteSpace(query.RiskLevel))
            filter.Add(new TermQuery(new Field("riskLevel")) { Value = query.RiskLevel.Trim() });
        if (query.IsActive.HasValue)
            filter.Add(new TermQuery(new Field("isActive")) { Value = query.IsActive.Value });

        return new BoolQuery
        {
            Must = must.Count > 0 ? must : null,
            Filter = filter.Count > 0 ? filter : null
        };
    }

    private static ICollection<SortOptions> BuildSort(CreditTypeSearchQuery query)
    {
        string key = (query.SortBy ?? "name").Trim().ToLowerInvariant();
        string field = key switch
        {
            "code" => "code.keyword",
            "locale" => "locale",
            "category" => "category",
            "currencycode" or "currency" => "currencyCode",
            "risklevel" or "risk" => "riskLevel",
            _ => "name.keyword"
        };
        return
        [
            SortOptions.Field(
                new Field(field),
                new FieldSort { Order = query.SortDesc ? SortOrder.Desc : SortOrder.Asc })
        ];
    }

    private static CreditTypeSearchDocument ToDocument(CreditType t) => new()
    {
        Id = t.Id,
        Name = t.Name,
        Code = t.Code,
        Locale = t.Locale,
        Category = t.Category.Name,
        Description = t.Description,
        CurrencyCode = t.CurrencyCode,
        RiskLevel = t.RiskLevel.Name,
        RequiresCollateral = t.RequiresCollateral,
        RequiresGuarantor = t.RequiresGuarantor,
        IsActive = t.IsActive
    };

    private static GetListCreditTypeListItemDto MapDto(CreditTypeSearchDocument d)
    {
        CreditCategory category = CreditCategory.TryFromName(d.Category, true, out CreditCategory? cat)
            ? cat!
            : CreditCategory.Individual;
        CreditRiskLevel risk = CreditRiskLevel.TryFromName(d.RiskLevel, true, out CreditRiskLevel? r)
            ? r!
            : CreditRiskLevel.Medium;

        return new GetListCreditTypeListItemDto
        {
            Id = d.Id,
            Name = d.Name,
            Code = d.Code,
            Locale = d.Locale,
            Category = category,
            CurrencyCode = d.CurrencyCode,
            RiskLevel = risk,
            IsActive = d.IsActive,
            IsDeleted = false
        };
    }
}
