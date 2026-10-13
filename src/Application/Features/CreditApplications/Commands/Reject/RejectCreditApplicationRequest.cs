using MediatR;

namespace Application.Features.CreditApplications.Commands.Reject;

public class RejectCreditApplicationRequest : IRequest<RejectedCreditApplicationResponse>
{
    public Guid Id { get; set; }
    public string RejectionReason { get; set; } = default!;
    public string? DecisionBy { get; set; }
    public string? Notes { get; set; }
}
