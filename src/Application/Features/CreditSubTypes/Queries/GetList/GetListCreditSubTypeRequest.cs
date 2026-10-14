using Application.Features.CreditSubTypes.Constants;
using Core.Application.Pipelines.Authorization;
using Core.Application.Pipelines.Caching;
using Core.Application.Requests;
using Core.Application.Responses;
using Core.Security.Constants;
using MediatR;

namespace Application.Features.CreditSubTypes.Queries.GetList;

public class GetListCreditSubTypeRequest
    : IRequest<GetListResponse<GetListCreditSubTypeListItemDto>>,
      ISecuredRequest,
      ICacheableRequest
{
    public string[] Roles => [GeneralOperationClaims.Customer, GeneralOperationClaims.BankStaff];
    public PageRequest PageRequest { get; set; } = new();
    public Guid? CreditTypeId { get; set; }

    public string CacheKey =>
        $"CreditSubTypeList({PageRequest.PageIndex},{PageRequest.PageSize},{CreditTypeId})";

    public string? CacheGroupKey => CreditSubTypeCacheGroupKeys.Group;
    public TimeSpan? SlidingExpiration => null;
}
