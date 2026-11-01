namespace Application.Services.Search;

public sealed class CreditApplicationSearchDocument
{
    public Guid Id { get; set; }
    public string ApplicationNumber { get; set; } = "";
    public string Status { get; set; } = "";
    public string? BranchId { get; set; }
    public string CustomerId { get; set; } = "";
    public string CreditSubTypeId { get; set; } = "";
    public string CreditSubTypeName { get; set; } = "";
    public string CurrencyCode { get; set; } = "USD";
    public decimal InterestRate { get; set; }
    public decimal RequestedAmount { get; set; }
    public int RequestedTermMonths { get; set; }
    public string? Notes { get; set; }
    public string? Purpose { get; set; }
    public string? DecisionBy { get; set; }
    public string? DecisionBranchCode { get; set; }
    public bool MissingRequiredDocuments { get; set; }
    public DateTime ApplicationDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
}
