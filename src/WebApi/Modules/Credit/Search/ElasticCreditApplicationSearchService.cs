using Application.Modules.Credit.Features.CreditApplicationDocuments;
using Application.Modules.Credit.Features.CreditApplications.Queries.GetList;
using Application.Services.Repositories;
using Application.Services.Search;
using Core.Application.Responses;
using Domain.Entities;
using Domain.Enums;
using Elastic.Clients.Elasticsearch;
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

        var created = await _client.Indices.CreateAsync(IndexName, cancellationToken);
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
        if (!IsAvailable || string.IsNullOrWhiteSpace(query.Text))
            return null;

        try
        {
            await EnsureIndexAsync(cancellationToken);

            string text = query.Text.Trim();
            string? branchScope = query.RequiredBranchScope?.ToString()
                ?? (query.AllowCrossBranchFilters ? query.BranchId?.ToString() : null);
            string? status = query.Status;
            bool missingOnly = query.MissingRequiredDocumentsOnly == true;

            int from = Math.Max(query.PageIndex, 0) * Math.Max(query.PageSize, 1);

            SearchResponse<CreditApplicationSearchDocument> response =
                await _client.SearchAsync<CreditApplicationSearchDocument>(s =>
                {
                    s.Index(IndexName).From(from).Size(query.PageSize);
                    s.Query(q => q.Bool(b =>
                    {
                        b.Must(m => m.MultiMatch(mm => mm
                            .Query(text)
                            .Fields(new[]
                            {
                                "applicationNumber^3",
                                "creditSubTypeName^2",
                                "notes",
                                "purpose",
                                "decisionBy",
                                "decisionBranchCode"
                            })
                            .Fuzziness(new Fuzziness("AUTO"))));

                        if (!string.IsNullOrEmpty(branchScope))
                            b.Filter(f => f.Term(t => t.Field(new Field("branchId")).Value(branchScope)));
                        if (!string.IsNullOrEmpty(status))
                            b.Filter(f => f.Term(t => t.Field(new Field("status")).Value(status)));
                        if (missingOnly)
                            b.Filter(f => f.Term(t => t.Field(new Field("missingRequiredDocuments")).Value(true)));
                    }));
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
