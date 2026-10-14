using Application.Features.CreditTypes.Constants;
using Core.Application.Pipelines.Authorization;
using Core.Application.Pipelines.Caching;
using Core.Security.Constants;
using MediatR;

namespace Application.Features.CreditTypes.Queries.GetById;

public class GetByIdCreditTypeRequest
    : IRequest<GetByIdCreditTypeResponse>,
      ISecuredRequest,
      ICacheableRequest
{
    public string[] Roles => [GeneralOperationClaims.Customer, GeneralOperationClaims.BankStaff];
    public Guid Id { get; set; }

    public string CacheKey => $"CreditType({Id})";
    public string? CacheGroupKey => CreditTypeCacheGroupKeys.Group;
    public TimeSpan? SlidingExpiration => null;
}
