using System.Linq.Expressions;
using Application.Services.Branches;
using Application.Services.Repositories;
using AutoMapper;
using Core.Application.Responses;
using Core.Repositories;
using Domain.CreditPackaging;
using Domain.Entities;
using Domain.Enums;
using MediatR;

namespace Application.Modules.Credit.Features.CreditSubTypes.Queries.GetList;

public class GetListCreditSubTypeRequestHandler
    : IRequestHandler<GetListCreditSubTypeRequest, GetListResponse<GetListCreditSubTypeListItemDto>>
{
    private readonly ICreditSubTypeRepository _creditSubTypeRepository;
    private readonly ICreditTypeRepository _creditTypeRepository;
    private readonly IMapper _mapper;
    private readonly ICatalogLocaleService _catalogLocale;

    public GetListCreditSubTypeRequestHandler(
        ICreditSubTypeRepository creditSubTypeRepository,
        ICreditTypeRepository creditTypeRepository,
        IMapper mapper,
        ICatalogLocaleService catalogLocale)
    {
        _creditSubTypeRepository = creditSubTypeRepository;
        _creditTypeRepository = creditTypeRepository;
        _mapper = mapper;
        _catalogLocale = catalogLocale;
    }

    public async Task<GetListResponse<GetListCreditSubTypeListItemDto>> Handle(
        GetListCreditSubTypeRequest request,
        CancellationToken cancellationToken)
    {
        string? forcedLocale = await _catalogLocale.ResolveForcedCatalogLocaleAsync(cancellationToken);
        string? effectiveLocale = forcedLocale ?? request.Locale;

        HashSet<Guid>? filteredTypeIds = null;
        bool filterTypes = !string.IsNullOrWhiteSpace(effectiveLocale) || !string.IsNullOrWhiteSpace(request.Category);

        if (filterTypes)
        {
            string? locale = string.IsNullOrWhiteSpace(effectiveLocale)
                ? null
                : effectiveLocale.Trim().ToLowerInvariant();
            CreditCategory? category = TryParseCategory(request.Category);

            IPaginate<CreditType> types = await _creditTypeRepository.GetListAsync(
                predicate: locale == null ? null : t => t.Locale == locale,
                size: 200,
                cancellationToken: cancellationToken);

            IEnumerable<CreditType> filtered = types.Items;
            if (category is not null)
                filtered = filtered.Where(t => t.Category == category);

            filteredTypeIds = filtered.Select(t => t.Id).ToHashSet();
        }

        Expression<Func<CreditSubType, bool>>? predicate = BuildPredicate(request.CreditTypeId, filteredTypeIds);

        IPaginate<CreditSubType> creditSubTypes = await _creditSubTypeRepository.GetListAsync(
            predicate: predicate,
            index: request.PageRequest.PageIndex,
            size: request.PageRequest.PageSize,
            cancellationToken: cancellationToken);

        GetListResponse<GetListCreditSubTypeListItemDto> response =
            _mapper.Map<GetListResponse<GetListCreditSubTypeListItemDto>>(creditSubTypes);

        await EnrichAsync(response.Items, cancellationToken);
        return response;
    }

    private async Task EnrichAsync(
        IList<GetListCreditSubTypeListItemDto> items,
        CancellationToken cancellationToken)
    {
        if (items.Count == 0)
            return;

        HashSet<Guid> typeIds = items.Select(i => i.CreditTypeId).ToHashSet();
        IPaginate<CreditType> types = await _creditTypeRepository.GetListAsync(
            predicate: t => typeIds.Contains(t.Id),
            size: Math.Max(typeIds.Count, 1),
            cancellationToken: cancellationToken);

        Dictionary<Guid, CreditType> byId = types.Items.ToDictionary(t => t.Id);
        foreach (GetListCreditSubTypeListItemDto item in items)
        {
            if (!byId.TryGetValue(item.CreditTypeId, out CreditType? type))
                continue;
            item.Locale = type.Locale;
            item.Category = type.Category.Name;
            item.CurrencyCode = string.IsNullOrWhiteSpace(type.CurrencyCode) ? "USD" : type.CurrencyCode;
            item.AmountOptions = CreditPackageConstraints
                .GetAmountOptions(item.MinAmount, item.MaxAmount, item.CurrencyCode)
                .ToList();
            item.TermOptions = CreditPackageConstraints
                .GetAllowedTermsInRange(item.MinTermMonths, item.MaxTermMonths)
                .ToList();
        }
    }

    private static CreditCategory? TryParseCategory(string? category)
    {
        if (string.IsNullOrWhiteSpace(category))
            return null;

        return CreditCategory.TryFromName(category.Trim(), ignoreCase: true, out CreditCategory? parsed)
            ? parsed
            : null;
    }

    private static Expression<Func<CreditSubType, bool>>? BuildPredicate(
        Guid? creditTypeId,
        HashSet<Guid>? filteredTypeIds)
    {
        if (creditTypeId.HasValue && filteredTypeIds is not null)
            return c => c.CreditTypeId == creditTypeId.Value && filteredTypeIds.Contains(c.CreditTypeId);

        if (creditTypeId.HasValue)
            return c => c.CreditTypeId == creditTypeId.Value;

        if (filteredTypeIds is not null)
            return c => filteredTypeIds.Contains(c.CreditTypeId);

        return null;
    }
}
