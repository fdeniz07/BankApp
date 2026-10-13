using Domain.Enums;

namespace Application.Features.CreditApplications.Queries.GetById;

public class GetByIdCreditApplicationResponse
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
    public DateTimeOffset? DecisionDate { get; set; }
    public decimal? ApprovedAmount { get; set; }
    public int? ApprovedTermMonths { get; set; }
    public decimal? ApprovedInterestRate { get; set; }
    public string? RejectionReason { get; set; }
    public string? Notes { get; set; }
    public string? DecisionBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
    public DateTimeOffset? UpdatedDate { get; set; }
}
