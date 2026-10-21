using Core.Repositories;
using Domain.Enums;
using Domain.Events;

namespace Domain.Entities;

public class CreditApplication : AggregateRoot
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

    public void InitializeAsNewApplication(string applicationNumber)
    {
        ApplicationNumber = applicationNumber;
        Status = CreditApplicationStatus.Pending;
        ApplicationDate = DateTimeOffset.UtcNow;

        Raise(new CreditApplicationCreated(
            Id,
            ApplicationNumber,
            CustomerId,
            CreditSubTypeId,
            RequestedAmount,
            RequestedTermMonths,
            DateTimeOffset.UtcNow));
    }

    public void Approve(
        decimal approvedAmount,
        int approvedTermMonths,
        decimal? approvedInterestRate,
        string? decisionBy,
        string? notes)
    {
        Status = CreditApplicationStatus.Approved;
        ApprovedAmount = approvedAmount;
        ApprovedTermMonths = approvedTermMonths;
        ApprovedInterestRate = approvedInterestRate;
        DecisionDate = DateTimeOffset.UtcNow;
        DecisionBy = decisionBy;
        Notes = notes ?? Notes;
        RejectionReason = null;

        Raise(new CreditApplicationApproved(
            Id,
            ApplicationNumber,
            CustomerId,
            approvedAmount,
            approvedTermMonths,
            approvedInterestRate,
            decisionBy,
            DateTimeOffset.UtcNow));
    }

    public void Reject(string rejectionReason, string? decisionBy, string? notes)
    {
        Status = CreditApplicationStatus.Rejected;
        RejectionReason = rejectionReason;
        DecisionDate = DateTimeOffset.UtcNow;
        DecisionBy = decisionBy;
        Notes = notes ?? Notes;
        ApprovedAmount = null;
        ApprovedTermMonths = null;
        ApprovedInterestRate = null;

        Raise(new CreditApplicationRejected(
            Id,
            ApplicationNumber,
            CustomerId,
            rejectionReason,
            decisionBy,
            DateTimeOffset.UtcNow));
    }

    public void Cancel(string? notes)
    {
        Status = CreditApplicationStatus.Cancelled;
        DecisionDate = DateTimeOffset.UtcNow;
        Notes = notes ?? Notes;
        ApprovedAmount = null;
        ApprovedTermMonths = null;
        ApprovedInterestRate = null;
        RejectionReason = null;

        Raise(new CreditApplicationCancelled(
            Id,
            ApplicationNumber,
            CustomerId,
            DateTimeOffset.UtcNow));
    }
}
