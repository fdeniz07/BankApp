using Core.Application.Pipelines.Authorization;
using Core.Security.Constants;
using MediatR;

namespace Application.Modules.Credit.Features.CreditApplications.Commands.Approve;

public class ApproveCreditApplicationRequest : IRequest<ApprovedCreditApplicationResponse>, ISecuredRequest
{
    public string[] Roles => [GeneralOperationClaims.BankStaff];
    public Guid Id { get; set; }
    public decimal ApprovedAmount { get; set; }
    public int ApprovedTermMonths { get; set; }
    public decimal ApprovedInterestRate { get; set; }
    public string? DecisionBy { get; set; }
    public string? Notes { get; set; }
    /// <summary>Letter template label for decision audit (system or user-saved name).</summary>
    public string? DecisionTemplateName { get; set; }
}
