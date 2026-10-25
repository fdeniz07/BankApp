namespace Application.Services.Amortization.Export;

public sealed class PaymentPlanExportDocument
{
    /// <summary>UI locale (tr / en / de). Drives PDF labels and number format.</summary>
    public string? Locale { get; init; }

    public string? ApplicationNumber { get; init; }
    public string? CustomerName { get; init; }
    public decimal Amount { get; init; }
    public int TermMonths { get; init; }
    public decimal InterestRate { get; init; }
    public string CurrencyCode { get; init; } = "TRY";
    public decimal MonthlyPayment { get; init; }
    public decimal TotalPayment { get; init; }
    public decimal TotalInterest { get; init; }
    public IReadOnlyList<PaymentPlanInstallment> Installments { get; init; } = [];
    public DateTime GeneratedAtUtc { get; init; } = DateTime.UtcNow;
}
