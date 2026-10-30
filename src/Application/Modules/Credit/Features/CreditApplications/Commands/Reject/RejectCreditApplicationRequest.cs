using Core.Application.Pipelines.Authorization;
using Core.Security.Constants;
using MediatR;

namespace Application.Modules.Credit.Features.CreditApplications.Commands.Reject;

public class RejectCreditApplicationRequest : IRequest<RejectedCreditApplicationResponse>, ISecuredRequest
{
    public string[] Roles => [GeneralOperationClaims.BankStaff];
    public Guid Id { get; set; }
    public string RejectionReason { get; set; } = default!;
    public string? DecisionBy { get; set; }
    public string? Notes { get; set; }
    /// <summary>Letter template label for decision audit (system or user-saved name).</summary>
    public string? DecisionTemplateName { get; set; }
}
