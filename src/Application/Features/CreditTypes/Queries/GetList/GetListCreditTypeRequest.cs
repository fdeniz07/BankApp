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

    public string CacheKey =>
        $"CreditTypeList({PageRequest.PageIndex},{PageRequest.PageSize})";

    public string? CacheGroupKey => CreditTypeCacheGroupKeys.Group;
    public TimeSpan? SlidingExpiration => null;
}
