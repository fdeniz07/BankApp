using Domain.Enums;

namespace Application.Modules.Credit.Features.CreditApplications.Commands.Approve;

public class ApprovedCreditApplicationResponse
{
    public Guid Id { get; set; }
    public string ApplicationNumber { get; set; } = default!;
    public CreditApplicationStatus Status { get; set; }
    public decimal? ApprovedAmount { get; set; }
    public int? ApprovedTermMonths { get; set; }
    public decimal? ApprovedInterestRate { get; set; }
    public DateTimeOffset? DecisionDate { get; set; }
    public string? DecisionBy { get; set; }
    public string? Notes { get; set; }
}
