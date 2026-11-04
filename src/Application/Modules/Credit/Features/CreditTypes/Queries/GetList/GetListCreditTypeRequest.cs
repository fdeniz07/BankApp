using Application.Modules.Credit.Features.CreditTypes.Constants;
using Core.Application.Pipelines.Authorization;
using Core.Application.Pipelines.Caching;
using Core.Application.Requests;
using Core.Application.Responses;
using Core.Security.Constants;
using MediatR;

namespace Application.Modules.Credit.Features.CreditTypes.Queries.GetList;

public class GetListCreditTypeRequest
    : IRequest<GetListResponse<GetListCreditTypeListItemDto>>,
      ISecuredRequest,
      ICacheableRequest
{
    public string[] Roles => [GeneralOperationClaims.Customer, GeneralOperationClaims.BankStaff];
    public PageRequest PageRequest { get; set; } = new();
    public string? Locale { get; set; }
    public string? Category { get; set; }
    public string? Search { get; set; }
    public string? CurrencyCode { get; set; }
    public string? RiskLevel { get; set; }
    public bool? IsActive { get; set; }
    public string? SortBy { get; set; }
    public bool SortDesc { get; set; }

    public string CacheKey =>
        $"CreditTypeList({PageRequest.PageIndex},{PageRequest.PageSize},{Normalize(Locale)},{Normalize(Category)},{Normalize(Search)},{Normalize(CurrencyCode)},{Normalize(RiskLevel)},{IsActive},{SortBy},{SortDesc})";

    public string? CacheGroupKey => CreditTypeCacheGroupKeys.Group;
    public TimeSpan? SlidingExpiration => null;

    private static string Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? "all" : value.Trim().ToLowerInvariant();
}
