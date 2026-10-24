using Application.Modules.Credit.Features.CreditSubTypes.Constants;
using Core.Application.Pipelines.Authorization;
using Core.Application.Pipelines.Caching;
using Core.Security.Constants;
using MediatR;

namespace Application.Modules.Credit.Features.CreditSubTypes.Queries.GetById;

public class GetByIdCreditSubTypeRequest
    : IRequest<GetByIdCreditSubTypeResponse>,
      ISecuredRequest,
      ICacheableRequest
{
    public string[] Roles => [GeneralOperationClaims.Customer, GeneralOperationClaims.BankStaff];
    public Guid Id { get; set; }

    public string CacheKey => $"CreditSubType({Id})";
    public string? CacheGroupKey => CreditSubTypeCacheGroupKeys.Group;
    public TimeSpan? SlidingExpiration => null;
}
