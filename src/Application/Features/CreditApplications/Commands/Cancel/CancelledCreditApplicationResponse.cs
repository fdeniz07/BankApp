using Domain.Enums;

namespace Application.Features.CreditApplications.Commands.Cancel;

public class CancelledCreditApplicationResponse
{
    public Guid Id { get; set; }
    public string ApplicationNumber { get; set; } = default!;
    public CreditApplicationStatus Status { get; set; }
    public DateTimeOffset? DecisionDate { get; set; }
    public string? Notes { get; set; }
}
