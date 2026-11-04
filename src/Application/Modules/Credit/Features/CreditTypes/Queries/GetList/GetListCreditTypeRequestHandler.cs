using System.Linq.Expressions;
using Application.Services.Branches;
using Application.Services.Repositories;
using Application.Services.Search;
using AutoMapper;
using Core.Application.Responses;
using Core.Repositories;
using Domain.Entities;
using Domain.Enums;
using MediatR;

namespace Application.Modules.Credit.Features.CreditTypes.Queries.GetList;

public class GetListCreditTypeRequestHandler
    : IRequestHandler<GetListCreditTypeRequest, GetListResponse<GetListCreditTypeListItemDto>>
{
    private readonly ICreditTypeRepository _creditTypeRepository;
    private readonly IMapper _mapper;
    private readonly ICatalogLocaleService _catalogLocale;
    private readonly ICreditTypeSearchService _search;

    public GetListCreditTypeRequestHandler(
        ICreditTypeRepository creditTypeRepository,
        IMapper mapper,
        ICatalogLocaleService catalogLocale,
        ICreditTypeSearchService search)
    {
        _creditTypeRepository = creditTypeRepository;
        _mapper = mapper;
        _catalogLocale = catalogLocale;
        _search = search;
    }

    public async Task<GetListResponse<GetListCreditTypeListItemDto>> Handle(
        GetListCreditTypeRequest request,
        CancellationToken cancellationToken)
    {
        string? forcedLocale = await _catalogLocale.ResolveForcedCatalogLocaleAsync(cancellationToken);
        string? locale = forcedLocale ?? request.Locale;

        bool preferSearch = _search.IsAvailable
            && (!string.IsNullOrWhiteSpace(request.Search)
                || !string.IsNullOrWhiteSpace(request.CurrencyCode)
                || !string.IsNullOrWhiteSpace(request.RiskLevel)
                || !string.IsNullOrWhiteSpace(locale)
                || !string.IsNullOrWhiteSpace(request.Category)
                || request.IsActive.HasValue);

        if (preferSearch)
        {
            GetListResponse<GetListCreditTypeListItemDto>? es = await _search.SearchAsync(
                new CreditTypeSearchQuery
                {
                    Text = request.Search ?? "",
                    PageIndex = request.PageRequest.PageIndex,
                    PageSize = request.PageRequest.PageSize,
                    Locale = locale,
                    Category = request.Category,
                    CurrencyCode = request.CurrencyCode,
                    RiskLevel = request.RiskLevel,
                    IsActive = request.IsActive,
                    SortBy = request.SortBy,
                    SortDesc = request.SortDesc
                },
                cancellationToken);
            if (es is not null)
                return es;
        }

        Expression<Func<CreditType, bool>>? predicate = BuildPredicate(
            locale,
            request.Category,
            request.Search,
            request.CurrencyCode,
            request.RiskLevel,
            request.IsActive);

        Func<IQueryable<CreditType>, IOrderedQueryable<CreditType>>? orderBy =
            BuildOrderBy(request.SortBy, request.SortDesc);

        IPaginate<CreditType> creditTypes = await _creditTypeRepository.GetListAsync(
            predicate: predicate,
            orderBy: orderBy,
            index: request.PageRequest.PageIndex,
            size: request.PageRequest.PageSize,
            cancellationToken: cancellationToken);

        return _mapper.Map<GetListResponse<GetListCreditTypeListItemDto>>(creditTypes);
    }

    private static Expression<Func<CreditType, bool>>? BuildPredicate(
        string? localeRaw,
        string? categoryRaw,
        string? searchRaw,
        string? currencyRaw,
        string? riskRaw,
        bool? isActive)
    {
        string? locale = string.IsNullOrWhiteSpace(localeRaw) ? null : localeRaw.Trim().ToLowerInvariant();
        CreditCategory? category = TryParseCategory(categoryRaw);
        string? search = string.IsNullOrWhiteSpace(searchRaw) ? null : searchRaw.Trim();
        string? currency = string.IsNullOrWhiteSpace(currencyRaw) ? null : currencyRaw.Trim().ToUpperInvariant();
        CreditRiskLevel? risk = TryParseRisk(riskRaw);

        if (locale is null && category is null && search is null && currency is null && risk is null && !isActive.HasValue)
            return null;

        return c =>
            (locale == null || c.Locale == locale)
            && (category == null || c.Category == category)
            && (currency == null || c.CurrencyCode == currency)
            && (risk == null || c.RiskLevel == risk)
            && (!isActive.HasValue || c.IsActive == isActive.Value)
            && (search == null
                || c.Name.Contains(search)
                || c.Code.Contains(search)
                || (c.Description != null && c.Description.Contains(search)));
    }

    private static CreditCategory? TryParseCategory(string? category)
    {
        if (string.IsNullOrWhiteSpace(category))
            return null;

        return CreditCategory.TryFromName(category.Trim(), ignoreCase: true, out CreditCategory? parsed)
            ? parsed
            : null;
    }

    private static CreditRiskLevel? TryParseRisk(string? risk)
    {
        if (string.IsNullOrWhiteSpace(risk))
            return null;

        return CreditRiskLevel.TryFromName(risk.Trim(), ignoreCase: true, out CreditRiskLevel? parsed)
            ? parsed
            : null;
    }

    private static Func<IQueryable<CreditType>, IOrderedQueryable<CreditType>>? BuildOrderBy(
        string? sortBy,
        bool sortDesc)
    {
        string key = (sortBy ?? "name").Trim().ToLowerInvariant();

        return key switch
        {
            "code" => sortDesc
                ? q => q.OrderByDescending(c => c.Code)
                : q => q.OrderBy(c => c.Code),
            "locale" => sortDesc
                ? q => q.OrderByDescending(c => c.Locale)
                : q => q.OrderBy(c => c.Locale),
            "category" => sortDesc
                ? q => q.OrderByDescending(c => c.Category)
                : q => q.OrderBy(c => c.Category),
            "currencycode" or "currency" => sortDesc
                ? q => q.OrderByDescending(c => c.CurrencyCode)
                : q => q.OrderBy(c => c.CurrencyCode),
            "risklevel" or "risk" => sortDesc
                ? q => q.OrderByDescending(c => c.RiskLevel)
                : q => q.OrderBy(c => c.RiskLevel),
            _ => sortDesc
                ? q => q.OrderByDescending(c => c.Name)
                : q => q.OrderBy(c => c.Name)
        };
    }
}
