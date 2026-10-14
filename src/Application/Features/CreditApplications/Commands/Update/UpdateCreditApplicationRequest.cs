using Core.Application.Pipelines.Authorization;
using Core.Security.Constants;
using MediatR;

namespace Application.Features.CreditApplications.Commands.Update;

public class UpdateCreditApplicationRequest : IRequest<UpdatedCreditApplicationResponse>, ISecuredRequest
{
    public string[] Roles => [GeneralOperationClaims.Customer, GeneralOperationClaims.BankStaff];
    public Guid Id { get; set; }
    public decimal RequestedAmount { get; set; }
    public int RequestedTermMonths { get; set; }
    public decimal? MonthlyIncome { get; set; }
    public decimal? ExistingDebtAmount { get; set; }
    public string? Purpose { get; set; }
    public string? Notes { get; set; }
}
