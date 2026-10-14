using Application.Features.CreditSubTypes.Constants;
using Core.Application.Pipelines.Authorization;
using Core.Application.Pipelines.Caching;
using Core.Security.Constants;
using MediatR;

namespace Application.Features.CreditSubTypes.Commands.Create;

public class CreateCreditSubTypeRequest : IRequest<CreatedCreditSubTypeResponse>, ISecuredRequest, ICacheRemoverRequest
{
    public string[] Roles => [GeneralOperationClaims.BankStaff];
    public string CacheGroupKey => CreditSubTypeCacheGroupKeys.Group;
    public Guid CreditTypeId { get; set; }
    public string Name { get; set; } = default!;
    public string Code { get; set; } = default!;
    public string? Description { get; set; }
    public decimal MinAmount { get; set; }
    public decimal MaxAmount { get; set; }
    public decimal InterestRate { get; set; }
    public int MinTermMonths { get; set; }
    public int MaxTermMonths { get; set; }
    public decimal? LateFeeRate { get; set; }
    public decimal? EarlyPaymentFeeRate { get; set; }
    public decimal? ProcessingFeeAmount { get; set; }
    public decimal? ProcessingFeeRate { get; set; }
    public int GracePeriodDays { get; set; }
    public bool RequiresCollateral { get; set; }
    public bool RequiresIncomeDocument { get; set; }
}
