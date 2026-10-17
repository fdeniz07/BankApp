using Application.Features.CreditTypes.Constants;
using Core.Application.Pipelines.Authorization;
using Core.Application.Pipelines.Caching;
using Core.Security.Constants;
using MediatR;

namespace Application.Features.CreditTypes.Commands.Delete;

public class DeleteCreditTypeRequest : IRequest<DeletedCreditTypeResponse>, ISecuredRequest, ICacheRemoverRequest
{
    public string[] Roles => [GeneralOperationClaims.BankManager];
    public string CacheGroupKey => CreditTypeCacheGroupKeys.Group;
    public Guid Id { get; set; }
}
