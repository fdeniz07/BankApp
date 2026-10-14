using Application.Features.CreditTypes.Constants;
using Core.Application.Pipelines.Authorization;
using Core.Application.Pipelines.Caching;
using Core.Security.Constants;
using Domain.Enums;
using MediatR;

namespace Application.Features.CreditTypes.Commands.Create;

public class CreateCreditTypeRequest : IRequest<CreatedCreditTypeResponse>, ISecuredRequest, ICacheRemoverRequest
{
    public string[] Roles => [GeneralOperationClaims.BankStaff];
    public string CacheGroupKey => CreditTypeCacheGroupKeys.Group;
    public string Name { get; set; } = default!;
    public string Code { get; set; } = default!;
    public CreditCategory Category { get; set; }
    public string? Description { get; set; }
    public string CurrencyCode { get; set; } = "TRY";
    public CreditRiskLevel RiskLevel { get; set; } = CreditRiskLevel.Medium;
    public bool RequiresCollateral { get; set; }
    public bool RequiresGuarantor { get; set; }
}
