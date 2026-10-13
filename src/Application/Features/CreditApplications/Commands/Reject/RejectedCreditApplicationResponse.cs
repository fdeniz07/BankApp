using Domain.Enums;

namespace Application.Features.CreditApplications.Commands.Reject;

public class RejectedCreditApplicationResponse
{
    public Guid Id { get; set; }
    public string ApplicationNumber { get; set; } = default!;
    public CreditApplicationStatus Status { get; set; }
    public string? RejectionReason { get; set; }
    public DateTimeOffset? DecisionDate { get; set; }
    public string? DecisionBy { get; set; }
    public string? Notes { get; set; }
}
