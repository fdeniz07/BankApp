using Application.Features.CreditTypes.Constants;
using Core.Application.Pipelines.Authorization;
using Core.Application.Pipelines.Caching;
using Core.Security.Constants;
using Domain.Enums;
using MediatR;

namespace Application.Features.CreditTypes.Commands.Update;

public class UpdateCreditTypeRequest : IRequest<UpdatedCreditTypeResponse>, ISecuredRequest, ICacheRemoverRequest
{
    public string[] Roles => [GeneralOperationClaims.BankStaff];
    public string CacheGroupKey => CreditTypeCacheGroupKeys.Group;
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Code { get; set; } = default!;
    public CreditCategory Category { get; set; }
    public string? Description { get; set; }
    public string CurrencyCode { get; set; } = "TRY";
    public CreditRiskLevel RiskLevel { get; set; }
    public bool RequiresCollateral { get; set; }
    public bool RequiresGuarantor { get; set; }
    public bool IsActive { get; set; }
}
