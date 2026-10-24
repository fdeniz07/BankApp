using Application.Modules.Credit.Features.CreditSubTypes.Constants;
using Core.Application.Pipelines.Authorization;
using Core.Application.Pipelines.Caching;
using Core.Application.Requests;
using Core.Application.Responses;
using Core.Security.Constants;
using MediatR;

namespace Application.Modules.Credit.Features.CreditSubTypes.Queries.GetList;

public class GetListCreditSubTypeRequest
    : IRequest<GetListResponse<GetListCreditSubTypeListItemDto>>,
      ISecuredRequest,
      ICacheableRequest
{
    public string[] Roles => [GeneralOperationClaims.Customer, GeneralOperationClaims.BankStaff];
    public PageRequest PageRequest { get; set; } = new();
    public Guid? CreditTypeId { get; set; }
    public string? Locale { get; set; }
    public string? Category { get; set; }

    public string CacheKey =>
        $"CreditSubTypeList({PageRequest.PageIndex},{PageRequest.PageSize},{CreditTypeId},{NormalizeLocale(Locale)},{NormalizeCategory(Category)})";

    public string? CacheGroupKey => CreditSubTypeCacheGroupKeys.Group;
    public TimeSpan? SlidingExpiration => null;

    private static string NormalizeLocale(string? locale) =>
        string.IsNullOrWhiteSpace(locale) ? "all" : locale.Trim().ToLowerInvariant();

    private static string NormalizeCategory(string? category) =>
        string.IsNullOrWhiteSpace(category) ? "all" : category.Trim().ToLowerInvariant();
}
