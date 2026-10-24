using Application.Modules.Credit.Features.CreditSubTypes.Constants;
using Core.Application.Pipelines.Authorization;
using Core.Application.Pipelines.Caching;
using Core.Security.Constants;
using MediatR;

namespace Application.Modules.Credit.Features.CreditSubTypes.Commands.Delete;

public class DeleteCreditSubTypeRequest : IRequest<DeletedCreditSubTypeResponse>, ISecuredRequest, ICacheRemoverRequest
{
    public string[] Roles => [GeneralOperationClaims.BankStaff];
    public string CacheGroupKey => CreditSubTypeCacheGroupKeys.Group;
    public Guid Id { get; set; }
}
