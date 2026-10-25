using Domain.Enums;

namespace Application.Modules.Credit.Features.CreditApplications.Commands.Cancel;

public class CancelledCreditApplicationResponse
{
    public Guid Id { get; set; }
    public string ApplicationNumber { get; set; } = default!;
    public CreditApplicationStatus Status { get; set; } = null!;
    public DateTimeOffset? DecisionDate { get; set; }
    public string? Notes { get; set; }
}
