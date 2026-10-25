using Domain.Enums;

namespace Application.Modules.Credit.Features.CreditApplications.Queries.GetById;

public class GetByIdCreditApplicationResponse
{
    public Guid Id { get; set; }
    public string ApplicationNumber { get; set; } = default!;
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = default!;
    public CreditCategory? CustomerCategory { get; set; }
    /// <summary>Bank customer number (e.g. CUS-…).</summary>
    public string? CustomerNumber { get; set; }
    /// <summary>Individual national ID (TCKN) or corporate tax number.</summary>
    public string? CustomerIdentityNumber { get; set; }
    public Guid CreditSubTypeId { get; set; }
    public string CreditSubTypeName { get; set; } = default!;
    public string CurrencyCode { get; set; } = "USD";
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
    /// <summary>Resolved staff display name for <see cref="DecisionBy"/> email.</summary>
    public string? DecisionByName { get; set; }
    /// <summary>StaffPosition SmartEnum name when the decision maker is a staff user.</summary>
    public string? DecisionByPosition { get; set; }
    public string? DecisionByEmail { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
    public DateTimeOffset? UpdatedDate { get; set; }
}
