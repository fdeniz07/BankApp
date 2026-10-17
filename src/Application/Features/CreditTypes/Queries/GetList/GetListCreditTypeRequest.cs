using Application.Features.CreditTypes.Constants;
using Core.Application.Pipelines.Authorization;
using Core.Application.Pipelines.Caching;
using Core.Application.Requests;
using Core.Application.Responses;
using Core.Security.Constants;
using MediatR;

namespace Application.Features.CreditTypes.Queries.GetList;

public class GetListCreditTypeRequest
    : IRequest<GetListResponse<GetListCreditTypeListItemDto>>,
      ISecuredRequest,
      ICacheableRequest
{
    public string[] Roles => [GeneralOperationClaims.Customer, GeneralOperationClaims.BankStaff];
    public PageRequest PageRequest { get; set; } = new();
    public string? Locale { get; set; }
    public string? Category { get; set; }
    public string? SortBy { get; set; }
    public bool SortDesc { get; set; }

    public string CacheKey =>
        $"CreditTypeList({PageRequest.PageIndex},{PageRequest.PageSize},{NormalizeLocale(Locale)},{NormalizeCategory(Category)},{SortBy},{SortDesc})";

    public string? CacheGroupKey => CreditTypeCacheGroupKeys.Group;
    public TimeSpan? SlidingExpiration => null;

    private static string NormalizeLocale(string? locale) =>
        string.IsNullOrWhiteSpace(locale) ? "all" : locale.Trim().ToLowerInvariant();

    private static string NormalizeCategory(string? category) =>
        string.IsNullOrWhiteSpace(category) ? "all" : category.Trim().ToLowerInvariant();
}
