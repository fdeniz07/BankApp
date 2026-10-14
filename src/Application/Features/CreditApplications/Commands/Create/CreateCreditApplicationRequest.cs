using Core.Application.Pipelines.Authorization;
using Core.Security.Constants;
using MediatR;

namespace Application.Features.CreditApplications.Commands.Create;

public class CreateCreditApplicationRequest : IRequest<CreatedCreditApplicationResponse>, ISecuredRequest
{
    public string[] Roles => [GeneralOperationClaims.Customer, GeneralOperationClaims.BankStaff];
    public Guid CustomerId { get; set; }
    public Guid CreditSubTypeId { get; set; }
    public decimal RequestedAmount { get; set; }
    public int RequestedTermMonths { get; set; }
    public decimal? MonthlyIncome { get; set; }
    public decimal? ExistingDebtAmount { get; set; }
    public string? Purpose { get; set; }
    public string? Notes { get; set; }
}
