using Core.Application.Pipelines.Authorization;
using Core.Security.Constants;
using MediatR;

namespace Application.Modules.Credit.Features.CreditApplications.Queries.GetPaymentPlan;

public class GetPaymentPlanRequest : IRequest<GetPaymentPlanResponse>, ISecuredRequest
{
    public string[] Roles => [GeneralOperationClaims.Customer, GeneralOperationClaims.BankStaff];
    public Guid? CreditSubTypeId { get; set; }
    public decimal Amount { get; set; }
    public int TermMonths { get; set; }
    public decimal? InterestRate { get; set; }
}
