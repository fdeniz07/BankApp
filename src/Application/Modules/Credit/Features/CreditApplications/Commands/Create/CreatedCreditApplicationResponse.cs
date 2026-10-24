using Domain.Enums;

namespace Application.Modules.Credit.Features.CreditApplications.Commands.Create;

public class CreatedCreditApplicationResponse
{
    public Guid Id { get; set; }
    public string ApplicationNumber { get; set; } = default!;
    public Guid CustomerId { get; set; }
    public Guid CreditSubTypeId { get; set; }
    public decimal RequestedAmount { get; set; }
    public int RequestedTermMonths { get; set; }
    public decimal? MonthlyIncome { get; set; }
    public decimal? ExistingDebtAmount { get; set; }
    public string? Purpose { get; set; }
    public CreditApplicationStatus Status { get; set; }
    public DateTimeOffset ApplicationDate { get; set; }
    public string? Notes { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
}
