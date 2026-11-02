using Application.Modules.Credit.Features.CreditApplicationDocuments;
using Application.Services.Auth;
using Application.Services.Repositories;
using Application.Services.Search;
using Core.Repositories;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Modules.Credit.Features.CreditApplications.Queries.GetFacets;

public sealed class GetCreditApplicationFacetsRequestHandler
    : IRequestHandler<GetCreditApplicationFacetsRequest, CreditApplicationFacetsResponse>
{
    private readonly ICreditApplicationSearchService _search;
    private readonly ICreditApplicationRepository _applications;
    private readonly ICreditSubTypeRepository _subTypes;
    private readonly ICreditTypeRepository _creditTypes;
    private readonly ICreditApplicationDocumentRepository _documents;
    private readonly IBranchRepository _branches;
    private readonly ICurrentUserService _currentUser;
    private readonly IBranchAuthorizationService _branchAuthorization;

    public GetCreditApplicationFacetsRequestHandler(
        ICreditApplicationSearchService search,
        ICreditApplicationRepository applications,
        ICreditSubTypeRepository subTypes,
        ICreditTypeRepository creditTypes,
        ICreditApplicationDocumentRepository documents,
        IBranchRepository branches,
        ICurrentUserService currentUser,
        IBranchAuthorizationService branchAuthorization)
    {
        _search = search;
        _applications = applications;
        _subTypes = subTypes;
        _creditTypes = creditTypes;
        _documents = documents;
        _branches = branches;
        _currentUser = currentUser;
        _branchAuthorization = branchAuthorization;
    }

    public async Task<CreditApplicationFacetsResponse> Handle(
        GetCreditApplicationFacetsRequest request,
        CancellationToken cancellationToken)
    {
        if (_branchAuthorization.MustSeeNoBranchData)
            return Empty("Sql");

        Guid? branchFilter = _branchAuthorization.ResolveListBranchFilter(request.BranchId);

        CreditApplicationSearchQuery query = new()
        {
            Text = request.SearchText ?? "",
            Status = request.Status?.Name,
            BranchId = request.BranchId,
            MissingRequiredDocumentsOnly = request.MissingRequiredDocumentsOnly,
            CurrencyCode = request.CurrencyCode,
            AmountMin = request.AmountMin,
            AmountMax = request.AmountMax,
            RequiredBranchScope = _currentUser.IsSuperAdmin ? null : branchFilter,
            AllowCrossBranchFilters = _currentUser.IsSuperAdmin || _currentUser.IsBankManager
        };

        if (_search.IsAvailable)
        {
            CreditApplicationFacetsResponse? es = await _search.GetFacetsAsync(query, cancellationToken);
            if (es is not null)
            {
                await EnrichBranchLabelsAsync(es, cancellationToken);
                return es;
            }
        }

        CreditApplicationFacetsResponse sql = await BuildSqlFacetsAsync(request, branchFilter, cancellationToken);
        await EnrichBranchLabelsAsync(sql, cancellationToken);
        return sql;
    }

    private async Task<CreditApplicationFacetsResponse> BuildSqlFacetsAsync(
        GetCreditApplicationFacetsRequest request,
        Guid? branchFilter,
        CancellationToken cancellationToken)
    {
        string? text = string.IsNullOrWhiteSpace(request.SearchText) ? null : request.SearchText.Trim();

        IPaginate<CreditApplication> page = await _applications.GetListAsync(
            predicate: c =>
                !c.IsDeleted
                && (request.Status == null || c.Status == request.Status)
                && (!branchFilter.HasValue || c.BranchId == branchFilter.Value)
                && (!request.AmountMin.HasValue || c.RequestedAmount >= request.AmountMin.Value)
                && (!request.AmountMax.HasValue || c.RequestedAmount < request.AmountMax.Value)
                && (text == null
                    || c.ApplicationNumber.Contains(text)
                    || (c.Notes != null && c.Notes.Contains(text))
                    || (c.Purpose != null && c.Purpose.Contains(text))
                    || (c.DecisionBy != null && c.DecisionBy.Contains(text))),
            include: q => q.Include(c => c.CreditSubType),
            index: 0,
            size: 500,
            cancellationToken: cancellationToken);

        List<CreditApplication> apps = page.Items.ToList();

        HashSet<Guid> missingIds = await ResolveMissingRequiredIdsAsync(apps, cancellationToken);
        if (request.MissingRequiredDocumentsOnly == true)
            apps = apps.Where(a => missingIds.Contains(a.Id)).ToList();

        Dictionary<Guid, string> currencyBySubType = await ResolveCurrencyBySubTypeAsync(apps, cancellationToken);
        if (!string.IsNullOrWhiteSpace(request.CurrencyCode))
        {
            string code = request.CurrencyCode.Trim().ToUpperInvariant();
            apps = apps
                .Where(a =>
                    (currencyBySubType.TryGetValue(a.CreditSubTypeId, out string? c) ? c : "USD") == code)
                .ToList();
        }

        CreditApplicationFacetsResponse result = new() { Source = "Sql" };

        result.Statuses = apps
            .GroupBy(a => a.Status.Name)
            .Select(g => new FacetBucketDto { Key = g.Key, Count = g.Count() })
            .OrderByDescending(b => b.Count)
            .ToList();

        result.Branches = apps
            .Where(a => a.BranchId.HasValue)
            .GroupBy(a => a.BranchId!.Value)
            .Select(g => new FacetBucketDto { Key = g.Key.ToString(), Count = g.Count() })
            .OrderByDescending(b => b.Count)
            .ToList();

        result.Currencies = apps
            .GroupBy(a => currencyBySubType.TryGetValue(a.CreditSubTypeId, out string? code) ? code : "USD")
            .Select(g => new FacetBucketDto { Key = g.Key, Count = g.Count() })
            .OrderByDescending(b => b.Count)
            .ToList();

        long missingTrue = apps.Count(a =>
            a.Status == CreditApplicationStatus.Pending && missingIds.Contains(a.Id));
        result.MissingRequiredDocuments = new FacetMissingDocsDto
        {
            True = missingTrue,
            False = apps.Count - missingTrue
        };

        result.AmountBuckets = CreditApplicationAmountBuckets.Ranges
            .Select((range, index) => new AmountBucketDto
            {
                From = range.From,
                To = range.To,
                Count = apps.Count(a => CreditApplicationAmountBuckets.BucketIndex(a.RequestedAmount) == index)
            })
            .ToList();

        return result;
    }

    private async Task<HashSet<Guid>> ResolveMissingRequiredIdsAsync(
        List<CreditApplication> apps,
        CancellationToken cancellationToken)
    {
        HashSet<Guid> pendingIds = apps
            .Where(a => a.Status == CreditApplicationStatus.Pending)
            .Select(a => a.Id)
            .ToHashSet();

        if (pendingIds.Count == 0)
            return [];

        IPaginate<CreditApplicationDocument> docs = await _documents.GetListAsync(
            predicate: d => pendingIds.Contains(d.CreditApplicationId) && !d.IsDeleted,
            size: Math.Max(pendingIds.Count * 10, 50),
            cancellationToken: cancellationToken);

        Dictionary<Guid, List<string>> byApp = docs.Items
            .GroupBy(d => d.CreditApplicationId)
            .ToDictionary(g => g.Key, g => g.Select(d => d.DocumentType.Name).ToList());

        return pendingIds
            .Where(id =>
                RequiredCreditDocuments.IsMissingRequired(
                    byApp.TryGetValue(id, out List<string>? types) ? types : []))
            .ToHashSet();
    }

    private async Task<Dictionary<Guid, string>> ResolveCurrencyBySubTypeAsync(
        List<CreditApplication> apps,
        CancellationToken cancellationToken)
    {
        HashSet<Guid> subTypeIds = apps.Select(a => a.CreditSubTypeId).ToHashSet();
        if (subTypeIds.Count == 0)
            return [];

        IPaginate<CreditSubType> subTypes = await _subTypes.GetListAsync(
            predicate: s => subTypeIds.Contains(s.Id),
            size: Math.Max(subTypeIds.Count, 1),
            cancellationToken: cancellationToken);

        HashSet<Guid> typeIds = subTypes.Items.Select(s => s.CreditTypeId).ToHashSet();
        IPaginate<CreditType> types = await _creditTypes.GetListAsync(
            predicate: t => typeIds.Contains(t.Id),
            size: Math.Max(typeIds.Count, 1),
            cancellationToken: cancellationToken);

        Dictionary<Guid, string> currencyByType = types.Items.ToDictionary(
            t => t.Id,
            t => string.IsNullOrWhiteSpace(t.CurrencyCode) ? "USD" : t.CurrencyCode);

        return subTypes.Items.ToDictionary(
            s => s.Id,
            s => currencyByType.TryGetValue(s.CreditTypeId, out string? code) ? code : "USD");
    }

    private async Task EnrichBranchLabelsAsync(
        CreditApplicationFacetsResponse response,
        CancellationToken cancellationToken)
    {
        HashSet<Guid> ids = response.Branches
            .Select(b => Guid.TryParse(b.Key, out Guid id) ? id : Guid.Empty)
            .Where(id => id != Guid.Empty)
            .ToHashSet();

        if (ids.Count == 0)
            return;

        IPaginate<Branch> branches = await _branches.GetListAsync(
            predicate: b => ids.Contains(b.Id),
            size: Math.Max(ids.Count, 1),
            cancellationToken: cancellationToken);

        Dictionary<Guid, Branch> byId = branches.Items.ToDictionary(b => b.Id);
        foreach (FacetBucketDto bucket in response.Branches)
        {
            if (Guid.TryParse(bucket.Key, out Guid id) && byId.TryGetValue(id, out Branch? branch))
                bucket.Label = string.IsNullOrWhiteSpace(branch.Code) ? branch.Name : branch.Code;
        }
    }

    private static CreditApplicationFacetsResponse Empty(string source) => new() { Source = source };
}
