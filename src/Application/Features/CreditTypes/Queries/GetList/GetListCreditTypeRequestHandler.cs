using System.Linq.Expressions;
using Application.Services.Repositories;
using AutoMapper;
using Core.Application.Responses;
using Core.Repositories;
using Domain.Entities;
using Domain.Enums;
using MediatR;

namespace Application.Features.CreditTypes.Queries.GetList;

public class GetListCreditTypeRequestHandler
    : IRequestHandler<GetListCreditTypeRequest, GetListResponse<GetListCreditTypeListItemDto>>
{
    private readonly ICreditTypeRepository _creditTypeRepository;
    private readonly IMapper _mapper;

    public GetListCreditTypeRequestHandler(ICreditTypeRepository creditTypeRepository, IMapper mapper)
    {
        _creditTypeRepository = creditTypeRepository;
        _mapper = mapper;
    }

    public async Task<GetListResponse<GetListCreditTypeListItemDto>> Handle(
        GetListCreditTypeRequest request,
        CancellationToken cancellationToken)
    {
        Expression<Func<CreditType, bool>>? predicate = BuildPredicate(request.Locale, request.Category);

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

    private static Expression<Func<CreditType, bool>>? BuildPredicate(string? localeRaw, string? categoryRaw)
    {
        string? locale = string.IsNullOrWhiteSpace(localeRaw)
            ? null
            : localeRaw.Trim().ToLowerInvariant();
        CreditCategory? category = TryParseCategory(categoryRaw);

        if (locale is null && category is null)
            return null;

        if (locale is not null && category is not null)
            return c => c.Locale == locale && c.Category == category;

        if (locale is not null)
            return c => c.Locale == locale;

        return c => c.Category == category!;
    }

    private static CreditCategory? TryParseCategory(string? category)
    {
        if (string.IsNullOrWhiteSpace(category))
            return null;

        return CreditCategory.TryFromName(category.Trim(), ignoreCase: true, out CreditCategory? parsed)
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
            "isactive" or "active" => sortDesc
                ? q => q.OrderByDescending(c => c.IsActive)
                : q => q.OrderBy(c => c.IsActive),
            _ => sortDesc
                ? q => q.OrderByDescending(c => c.Name)
                : q => q.OrderBy(c => c.Name)
        };
    }
}
