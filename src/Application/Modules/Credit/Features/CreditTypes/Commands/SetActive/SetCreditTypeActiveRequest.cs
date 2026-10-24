using Application.Modules.Credit.Features.CreditTypes.Constants;
using Core.Application.Pipelines.Authorization;
using Core.Application.Pipelines.Caching;
using Core.Security.Constants;
using MediatR;

namespace Application.Modules.Credit.Features.CreditTypes.Commands.SetActive;

public class SetCreditTypeActiveRequest
    : IRequest<SetCreditTypeActiveResponse>,
      ISecuredRequest,
      ICacheRemoverRequest
{
    public string[] Roles => [GeneralOperationClaims.BankManager];
    public string CacheGroupKey => CreditTypeCacheGroupKeys.Group;
    public Guid Id { get; set; }
    public bool IsActive { get; set; }
}
