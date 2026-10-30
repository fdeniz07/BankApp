using Core.Repositories;
using Domain.Enums;
using Domain.Events;

namespace Domain.Entities;

public class CreditApplication : AggregateRoot
{
    public string ApplicationNumber { get; set; } = default!;
    public Guid CustomerId { get; set; }
    /// <summary>Branch owning this application (copied from customer at create).</summary>
    public Guid? BranchId { get; set; }
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
    /// <summary>Branch code at decision time (snapshot; not a live FK).</summary>
    public string? DecisionBranchCode { get; set; }
    /// <summary>Branch display name at decision time (snapshot).</summary>
    public string? DecisionBranchName { get; set; }
    /// <summary>User/system letter template name used for the decision letter (audit).</summary>
    public string? DecisionTemplateName { get; set; }

    public Customer Customer { get; set; } = default!;
    public Branch? Branch { get; set; }
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
        string? notes,
        string? decisionBranchCode = null,
        string? decisionBranchName = null,
        string? decisionTemplateName = null)
    {
        Status = CreditApplicationStatus.Approved;
        ApprovedAmount = approvedAmount;
        ApprovedTermMonths = approvedTermMonths;
        ApprovedInterestRate = approvedInterestRate;
        DecisionDate = DateTimeOffset.UtcNow;
        DecisionBy = decisionBy;
        DecisionBranchCode = decisionBranchCode;
        DecisionBranchName = decisionBranchName;
        DecisionTemplateName = TrimTemplate(decisionTemplateName);
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

    public void Reject(
        string rejectionReason,
        string? decisionBy,
        string? notes,
        string? decisionBranchCode = null,
        string? decisionBranchName = null,
        string? decisionTemplateName = null)
    {
        Status = CreditApplicationStatus.Rejected;
        RejectionReason = rejectionReason;
        DecisionDate = DateTimeOffset.UtcNow;
        DecisionBy = decisionBy;
        DecisionBranchCode = decisionBranchCode;
        DecisionBranchName = decisionBranchName;
        DecisionTemplateName = TrimTemplate(decisionTemplateName);
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

    /// <summary>Raises realtime/outbox event after a document is stored (aggregate stays source of truth for messaging).</summary>
    public void RecordDocumentUploaded(string documentType)
    {
        Raise(new CreditApplicationDocumentUploaded(
            Id,
            ApplicationNumber,
            CustomerId,
            documentType,
            DateTimeOffset.UtcNow));
    }

    private static string? TrimTemplate(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;
        string trimmed = name.Trim();
        return trimmed.Length > 120 ? trimmed[..120] : trimmed;
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
