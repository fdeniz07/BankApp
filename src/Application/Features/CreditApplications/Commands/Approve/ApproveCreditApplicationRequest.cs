using Core.Application.Pipelines.Authorization;
using Core.Security.Constants;
using MediatR;

namespace Application.Features.CreditApplications.Commands.Approve;

public class ApproveCreditApplicationRequest : IRequest<ApprovedCreditApplicationResponse>, ISecuredRequest
{
    public string[] Roles => [GeneralOperationClaims.BankStaff];
    public Guid Id { get; set; }
    public decimal ApprovedAmount { get; set; }
    public int ApprovedTermMonths { get; set; }
    public decimal ApprovedInterestRate { get; set; }
    public string? DecisionBy { get; set; }
    public string? Notes { get; set; }
}
