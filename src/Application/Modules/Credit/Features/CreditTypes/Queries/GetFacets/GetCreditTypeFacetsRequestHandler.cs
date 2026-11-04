using Application.Modules.Credit.Features.CreditApplications.Queries.GetFacets;
using Application.Services.Branches;
using Application.Services.Repositories;
using Application.Services.Search;
using Domain.Entities;
using Domain.Enums;
using MediatR;

namespace Application.Modules.Credit.Features.CreditTypes.Queries.GetFacets;

public class GetCreditTypeFacetsRequestHandler
    : IRequestHandler<GetCreditTypeFacetsRequest, CreditTypeFacetsResponse>
{
    private readonly ICreditTypeRepository _creditTypes;
    private readonly ICatalogLocaleService _catalogLocale;
    private readonly ICreditTypeSearchService _search;

    public GetCreditTypeFacetsRequestHandler(
        ICreditTypeRepository creditTypes,
        ICatalogLocaleService catalogLocale,
        ICreditTypeSearchService search)
    {
        _creditTypes = creditTypes;
        _catalogLocale = catalogLocale;
        _search = search;
    }

    public async Task<CreditTypeFacetsResponse> Handle(
        GetCreditTypeFacetsRequest request,
        CancellationToken cancellationToken)
    {
        string? forcedLocale = await _catalogLocale.ResolveForcedCatalogLocaleAsync(cancellationToken);
        string? locale = forcedLocale ?? request.Locale;

        CreditTypeSearchQuery query = new()
        {
            Text = request.Search ?? "",
            Locale = locale,
            Category = request.Category,
            CurrencyCode = request.CurrencyCode,
            RiskLevel = request.RiskLevel,
            IsActive = request.IsActive,
            PageSize = 0
        };

        CreditTypeFacetsResponse? es = await _search.GetFacetsAsync(query, cancellationToken);
        if (es is not null)
            return es;

        return await BuildSqlFacetsAsync(query, cancellationToken);
    }

    private async Task<CreditTypeFacetsResponse> BuildSqlFacetsAsync(
        CreditTypeSearchQuery query,
        CancellationToken cancellationToken)
    {
        string? locale = string.IsNullOrWhiteSpace(query.Locale) ? null : query.Locale.Trim().ToLowerInvariant();
        CreditCategory? category = CreditCategory.TryFromName(query.Category ?? "", true, out CreditCategory? cat)
            ? cat
            : null;
        string? currency = string.IsNullOrWhiteSpace(query.CurrencyCode)
            ? null
            : query.CurrencyCode.Trim().ToUpperInvariant();
        CreditRiskLevel? risk = CreditRiskLevel.TryFromName(query.RiskLevel ?? "", true, out CreditRiskLevel? r)
            ? r
            : null;
        string? search = string.IsNullOrWhiteSpace(query.Text) ? null : query.Text.Trim();

        var page = await _creditTypes.GetListAsync(
            predicate: c =>
                (locale == null || c.Locale == locale)
                && (category == null || c.Category == category)
                && (currency == null || c.CurrencyCode == currency)
                && (risk == null || c.RiskLevel == risk)
                && (!query.IsActive.HasValue || c.IsActive == query.IsActive.Value)
                && (search == null
                    || c.Name.Contains(search)
                    || c.Code.Contains(search)
                    || (c.Description != null && c.Description.Contains(search))),
            index: 0,
            size: 500,
            cancellationToken: cancellationToken);

        return new CreditTypeFacetsResponse
        {
            Source = "Sql",
            Locales = page.Items.GroupBy(c => c.Locale)
                .Select(g => new FacetBucketDto { Key = g.Key, Count = g.Count() }).ToList(),
            Categories = page.Items.GroupBy(c => c.Category.Name)
                .Select(g => new FacetBucketDto { Key = g.Key, Count = g.Count() }).ToList(),
            Currencies = page.Items.GroupBy(c => c.CurrencyCode)
                .Select(g => new FacetBucketDto { Key = g.Key, Count = g.Count() }).ToList(),
            RiskLevels = page.Items.GroupBy(c => c.RiskLevel.Name)
                .Select(g => new FacetBucketDto { Key = g.Key, Count = g.Count() }).ToList()
        };
    }
}
