using Domain.Enums;
using Core.Application.Pipelines.Authorization;
using Core.Security.Constants;
using MediatR;

namespace Application.Features.CreditTypes.Commands.Create;

public class CreateCreditTypeRequest : IRequest<CreatedCreditTypeResponse>, ISecuredRequest
{
    public string[] Roles => [GeneralOperationClaims.BankStaff];
    public string Name { get; set; } = default!;
    public string Code { get; set; } = default!;
    public CreditCategory Category { get; set; }
    public string? Description { get; set; }
    public string CurrencyCode { get; set; } = "TRY";
    public CreditRiskLevel RiskLevel { get; set; } = CreditRiskLevel.Medium;
    public bool RequiresCollateral { get; set; }
    public bool RequiresGuarantor { get; set; }
}
