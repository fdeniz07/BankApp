using Application.Modules.Credit.Features.CreditApplicationDocuments;
using Application.Modules.Credit.Features.CreditApplications.Queries.GetFacets;
using Application.Modules.Credit.Features.CreditApplications.Queries.GetList;
using Application.Services.Repositories;
using Application.Services.Search;
using Core.Application.Responses;
using Domain.Entities;
using Domain.Enums;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Aggregations;
using Elastic.Clients.Elasticsearch.Mapping;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace WebApi.Modules.Credit.Search;

public sealed class ElasticCreditApplicationSearchService : ICreditApplicationSearchService
{
    private readonly ElasticsearchClient _client;
    private readonly ElasticsearchOptions _options;
    private readonly ICreditApplicationRepository _applications;
    private readonly ICreditSubTypeRepository _subTypes;
    private readonly ICreditTypeRepository _creditTypes;
    private readonly ICreditApplicationDocumentRepository _documents;
    private readonly ILogger<ElasticCreditApplicationSearchService> _logger;

    public ElasticCreditApplicationSearchService(
        ElasticsearchClient client,
        IOptions<ElasticsearchOptions> options,
        ICreditApplicationRepository applications,
        ICreditSubTypeRepository subTypes,
        ICreditTypeRepository creditTypes,
        ICreditApplicationDocumentRepository documents,
        ILogger<ElasticCreditApplicationSearchService> logger)
    {
        _client = client;
        _options = options.Value;
        _applications = applications;
        _subTypes = subTypes;
        _creditTypes = creditTypes;
        _documents = documents;
        _logger = logger;
    }

    public bool IsAvailable => _options.Enabled;

    private string IndexName => _options.ApplicationsIndex;

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
                    { "applicationNumber", new TextProperty { Fields = new Properties { { "keyword", new KeywordProperty() } } } },
                    { "status", new KeywordProperty() },
                    { "branchId", new KeywordProperty() },
                    { "customerId", new KeywordProperty() },
                    { "creditSubTypeId", new KeywordProperty() },
                    { "creditSubTypeName", new TextProperty { Fields = new Properties { { "keyword", new KeywordProperty() } } } },
                    { "currencyCode", new KeywordProperty() },
                    { "interestRate", new DoubleNumberProperty() },
                    { "requestedAmount", new DoubleNumberProperty() },
                    { "requestedTermMonths", new IntegerNumberProperty() },
                    { "notes", new TextProperty() },
                    { "purpose", new TextProperty() },
                    { "decisionBy", new TextProperty { Fields = new Properties { { "keyword", new KeywordProperty() } } } },
                    { "decisionBranchCode", new KeywordProperty() },
                    { "missingRequiredDocuments", new BooleanProperty() },
                    { "applicationDate", new DateProperty() },
                    { "updatedDate", new DateProperty() }
                })), cancellationToken);

        if (!created.IsValidResponse)
            _logger.LogWarning("Elasticsearch index create issue: {Error}", created.DebugInformation);
    }

    public async Task IndexAsync(CreditApplicationSearchDocument document, CancellationToken cancellationToken = default)
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
            _logger.LogWarning(ex, "Failed to index credit application {Id}", document.Id);
        }
    }

    public async Task UpsertFromApplicationAsync(CreditApplication application, CancellationToken cancellationToken = default)
    {
        if (!IsAvailable)
            return;

        CreditApplicationSearchDocument doc = await BuildDocumentAsync(application, cancellationToken);
        await IndexAsync(doc, cancellationToken);
    }

    public async Task DeleteAsync(Guid applicationId, CancellationToken cancellationToken = default)
    {
        if (!IsAvailable)
            return;

        try
        {
            await _client.DeleteAsync(
                new DeleteRequest(IndexName, applicationId.ToString()),
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete credit application {Id} from index", applicationId);
        }
    }

    public async Task ReindexAllAsync(CancellationToken cancellationToken = default)
    {
        if (!IsAvailable)
            return;

        await EnsureIndexAsync(cancellationToken);

        var page = await _applications.GetListAsync(
            predicate: a => !a.IsDeleted,
            include: q => q.Include(a => a.CreditSubType),
            index: 0,
            size: 500,
            cancellationToken: cancellationToken);

        foreach (CreditApplication app in page.Items)
            await UpsertFromApplicationAsync(app, cancellationToken);

        _logger.LogInformation("Reindexed {Count} credit applications into {Index}", page.Items.Count, IndexName);
    }

    public async Task<GetListResponse<GetListCreditApplicationListItemDto>?> SearchAsync(
        CreditApplicationSearchQuery query,
        CancellationToken cancellationToken = default)
    {
        if (!IsAvailable)
            return null;

        bool hasText = !string.IsNullOrWhiteSpace(query.Text);
        bool hasFilters = !string.IsNullOrWhiteSpace(query.Status)
            || query.BranchId.HasValue
            || query.RequiredBranchScope.HasValue
            || query.MissingRequiredDocumentsOnly == true
            || !string.IsNullOrWhiteSpace(query.CurrencyCode)
            || query.AmountMin.HasValue
            || query.AmountMax.HasValue;

        if (!hasText && !hasFilters)
            return null;

        try
        {
            await EnsureIndexAsync(cancellationToken);

            int from = Math.Max(query.PageIndex, 0) * Math.Max(query.PageSize, 1);
            BoolQuery boolQuery = BuildBoolQuery(query);

            SearchResponse<CreditApplicationSearchDocument> response =
                await _client.SearchAsync<CreditApplicationSearchDocument>(s =>
                {
                    s.Index(IndexName).From(from).Size(query.PageSize);
                    s.Query(q => q.Bool(boolQuery));
                }, cancellationToken);

            if (!response.IsValidResponse)
            {
                _logger.LogWarning("Elasticsearch search failed: {Info}", response.DebugInformation);
                return null;
            }

            long total = response.Total;
            List<GetListCreditApplicationListItemDto> items = response.Documents.Select(MapDto).ToList();
            int size = Math.Max(query.PageSize, 1);
            int pages = size == 0 ? 0 : (int)Math.Ceiling(total / (double)size);

            return new GetListResponse<GetListCreditApplicationListItemDto>
            {
                Items = items,
                Index = query.PageIndex,
                Size = size,
                Count = (int)Math.Min(total, int.MaxValue),
                Pages = pages,
                From = 0
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Elasticsearch search unavailable; caller should fall back to SQL");
            return null;
        }
    }

    public async Task<CreditApplicationFacetsResponse?> GetFacetsAsync(
        CreditApplicationSearchQuery query,
        CancellationToken cancellationToken = default)
    {
        if (!IsAvailable)
            return null;

        try
        {
            await EnsureIndexAsync(cancellationToken);
            BoolQuery boolQuery = BuildBoolQuery(query);

            var ranges = CreditApplicationAmountBuckets.Ranges
                .Select(r => new AggregationRange
                {
                    From = (double)r.From,
                    To = r.To.HasValue ? (double)r.To.Value : null
                })
                .ToList();

            SearchResponse<CreditApplicationSearchDocument> response =
                await _client.SearchAsync<CreditApplicationSearchDocument>(s =>
                {
                    s.Index(IndexName).Size(0);
                    s.Query(q => q.Bool(boolQuery));
                    s.Aggregations(a =>
                    {
                        a.Add("statuses", agg => agg.Terms(t => t.Field("status").Size(20)));
                        a.Add("branches", agg => agg.Terms(t => t.Field("branchId").Size(50)));
                        a.Add("currencies", agg => agg.Terms(t => t.Field("currencyCode").Size(20)));
                        a.Add("missingDocs", agg => agg.Terms(t => t.Field("missingRequiredDocuments").Size(2)));
                        a.Add("amounts", agg => agg.Range(r => r.Field("requestedAmount").Ranges(ranges.ToArray())));
                        return a;
                    });
                }, cancellationToken);

            if (!response.IsValidResponse || response.Aggregations is null)
            {
                _logger.LogWarning("Elasticsearch facets failed: {Info}", response.DebugInformation);
                return null;
            }

            CreditApplicationFacetsResponse result = new() { Source = "Elasticsearch" };

            if (response.Aggregations.TryGetValue("statuses", out IAggregate? statusAgg)
                && statusAgg is StringTermsAggregate statusTerms)
            {
                result.Statuses = statusTerms.Buckets
                    .Select(b => new FacetBucketDto { Key = b.Key.ToString(), Count = b.DocCount })
                    .ToList();
            }

            if (response.Aggregations.TryGetValue("branches", out IAggregate? branchAgg)
                && branchAgg is StringTermsAggregate branchTerms)
            {
                result.Branches = branchTerms.Buckets
                    .Select(b => new FacetBucketDto { Key = b.Key.ToString(), Count = b.DocCount })
                    .ToList();
            }

            if (response.Aggregations.TryGetValue("currencies", out IAggregate? currencyAgg)
                && currencyAgg is StringTermsAggregate currencyTerms)
            {
                result.Currencies = currencyTerms.Buckets
                    .Select(b => new FacetBucketDto { Key = b.Key.ToString(), Count = b.DocCount })
                    .ToList();
            }

            if (response.Aggregations.TryGetValue("missingDocs", out IAggregate? missingAgg))
            {
                if (missingAgg is LongTermsAggregate longTerms)
                {
                    foreach (LongTermsBucket b in longTerms.Buckets)
                    {
                        if (b.Key == 1) result.MissingRequiredDocuments.True = b.DocCount;
                        else if (b.Key == 0) result.MissingRequiredDocuments.False = b.DocCount;
                    }
                }
                else if (missingAgg is StringTermsAggregate stringTerms)
                {
                    foreach (StringTermsBucket b in stringTerms.Buckets)
                    {
                        string key = b.Key.ToString();
                        if (key is "true" or "1") result.MissingRequiredDocuments.True = b.DocCount;
                        else if (key is "false" or "0") result.MissingRequiredDocuments.False = b.DocCount;
                    }
                }
            }

            if (response.Aggregations.TryGetValue("amounts", out IAggregate? amountAgg)
                && amountAgg is RangeAggregate rangeAgg)
            {
                result.AmountBuckets = rangeAgg.Buckets
                    .Select(b => new AmountBucketDto
                    {
                        From = (decimal)(b.From ?? 0),
                        To = b.To.HasValue ? (decimal)b.To.Value : null,
                        Count = b.DocCount
                    })
                    .ToList();
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Elasticsearch facets unavailable; caller should fall back to SQL");
            return null;
        }
    }

    private static BoolQuery BuildBoolQuery(CreditApplicationSearchQuery query)
    {
        List<Query> must = [];
        List<Query> filter = [];

        if (!string.IsNullOrWhiteSpace(query.Text))
        {
            string text = query.Text.Trim();
            must.Add(new MultiMatchQuery
            {
                Query = text,
                Fields = new[]
                {
                    "applicationNumber^3",
                    "creditSubTypeName^2",
                    "notes",
                    "purpose",
                    "decisionBy",
                    "decisionBranchCode"
                },
                Fuzziness = new Fuzziness("AUTO")
            });
        }

        string? branchScope = query.RequiredBranchScope?.ToString()
            ?? (query.AllowCrossBranchFilters ? query.BranchId?.ToString() : null);
        if (!string.IsNullOrEmpty(branchScope))
            filter.Add(new TermQuery(new Field("branchId")) { Value = branchScope });

        if (!string.IsNullOrEmpty(query.Status))
            filter.Add(new TermQuery(new Field("status")) { Value = query.Status });

        if (query.MissingRequiredDocumentsOnly == true)
            filter.Add(new TermQuery(new Field("missingRequiredDocuments")) { Value = true });

        if (!string.IsNullOrWhiteSpace(query.CurrencyCode))
            filter.Add(new TermQuery(new Field("currencyCode")) { Value = query.CurrencyCode.Trim().ToUpperInvariant() });

        if (query.AmountMin.HasValue || query.AmountMax.HasValue)
        {
            var nr = new NumberRangeQuery(new Field("requestedAmount"));
            if (query.AmountMin.HasValue)
                nr.Gte = (double)query.AmountMin.Value;
            if (query.AmountMax.HasValue)
                nr.Lt = (double)query.AmountMax.Value;
            filter.Add(nr);
        }

        return new BoolQuery
        {
            Must = must.Count > 0 ? must : null,
            Filter = filter.Count > 0 ? filter : null
        };
    }

    private async Task<CreditApplicationSearchDocument> BuildDocumentAsync(
        CreditApplication app,
        CancellationToken cancellationToken)
    {
        string subTypeName = app.CreditSubType?.Name ?? "";
        string currency = "USD";
        decimal interest = app.CreditSubType?.InterestRate ?? 0m;

        if (app.CreditSubType is null)
        {
            CreditSubType? sub = await _subTypes.GetAsync(
                s => s.Id == app.CreditSubTypeId,
                cancellationToken: cancellationToken);
            subTypeName = sub?.Name ?? "";
            interest = sub?.InterestRate ?? 0m;
            if (sub is not null)
            {
                CreditType? type = await _creditTypes.GetAsync(
                    t => t.Id == sub.CreditTypeId,
                    cancellationToken: cancellationToken);
                currency = string.IsNullOrWhiteSpace(type?.CurrencyCode) ? "USD" : type!.CurrencyCode;
            }
        }
        else
        {
            CreditType? type = await _creditTypes.GetAsync(
                t => t.Id == app.CreditSubType.CreditTypeId,
                cancellationToken: cancellationToken);
            currency = string.IsNullOrWhiteSpace(type?.CurrencyCode) ? "USD" : type!.CurrencyCode;
        }

        bool missingDocs = false;
        if (app.Status == CreditApplicationStatus.Pending)
        {
            var docs = await _documents.GetListAsync(
                predicate: d => d.CreditApplicationId == app.Id && !d.IsDeleted,
                size: 50,
                cancellationToken: cancellationToken);
            missingDocs = RequiredCreditDocuments.IsMissingRequired(
                docs.Items.Select(d => d.DocumentType.Name));
        }

        return new CreditApplicationSearchDocument
        {
            Id = app.Id,
            ApplicationNumber = app.ApplicationNumber,
            Status = app.Status.Name,
            BranchId = app.BranchId?.ToString(),
            CustomerId = app.CustomerId.ToString(),
            CreditSubTypeId = app.CreditSubTypeId.ToString(),
            CreditSubTypeName = subTypeName,
            CurrencyCode = currency,
            InterestRate = interest,
            RequestedAmount = app.RequestedAmount,
            RequestedTermMonths = app.RequestedTermMonths,
            Notes = app.Notes,
            Purpose = app.Purpose,
            DecisionBy = app.DecisionBy,
            DecisionBranchCode = app.DecisionBranchCode,
            MissingRequiredDocuments = missingDocs,
            ApplicationDate = app.ApplicationDate.UtcDateTime,
            UpdatedDate = app.UpdatedDate?.UtcDateTime
        };
    }

    private static GetListCreditApplicationListItemDto MapDto(CreditApplicationSearchDocument d)
    {
        CreditApplicationStatus status = CreditApplicationStatus.Pending;
        try
        {
            status = CreditApplicationStatus.FromName(d.Status, ignoreCase: true);
        }
        catch
        {
            // keep Pending
        }

        _ = Guid.TryParse(d.CustomerId, out Guid customerId);
        _ = Guid.TryParse(d.CreditSubTypeId, out Guid subTypeId);
        Guid? branchId = Guid.TryParse(d.BranchId, out Guid b) ? b : null;

        return new GetListCreditApplicationListItemDto
        {
            Id = d.Id,
            ApplicationNumber = d.ApplicationNumber,
            CustomerId = customerId,
            BranchId = branchId,
            CreditSubTypeId = subTypeId,
            CreditSubTypeName = d.CreditSubTypeName,
            CurrencyCode = d.CurrencyCode,
            RequestedAmount = d.RequestedAmount,
            RequestedTermMonths = d.RequestedTermMonths,
            InterestRate = d.InterestRate,
            Status = status,
            ApplicationDate = d.ApplicationDate,
            DecisionBy = d.DecisionBy,
            DecisionBranchCode = d.DecisionBranchCode,
            MissingRequiredDocuments = d.MissingRequiredDocuments
        };
    }
}
