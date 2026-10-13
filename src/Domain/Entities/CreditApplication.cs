using Core.Repositories;
using Domain.Enums;

namespace Domain.Entities;

public class CreditApplication : Entity
{
    public string ApplicationNumber { get; set; } = default!;
    public Guid CustomerId { get; set; }
    public Guid CreditSubTypeId { get; set; }
    public decimal RequestedAmount { get; set; }
    public int RequestedTermMonths { get; set; }
    public decimal? MonthlyIncome { get; set; }
    public decimal? ExistingDebtAmount { get; set; }
    public string? Purpose { get; set; }
    public CreditApplicationStatus Status { get; set; } = CreditApplicationStatus.Pending;
    public DateTimeOffset ApplicationDate { get; set; }
    public DateTimeOffset? DecisionDate { get; set; }
    public decimal? ApprovedAmount { get; set; }
    public int? ApprovedTermMonths { get; set; }
    public decimal? ApprovedInterestRate { get; set; }
    public string? RejectionReason { get; set; }
    public string? Notes { get; set; }
    public string? DecisionBy { get; set; }

    public Customer Customer { get; set; } = default!;
    public CreditSubType CreditSubType { get; set; } = default!;
}
