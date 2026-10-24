using Application.Services.Amortization;

namespace Application.Modules.Credit.Features.CreditApplications.Queries.GetPaymentPlan;

public class GetPaymentPlanResponse
{
    public decimal Amount { get; set; }
    public int TermMonths { get; set; }
    public decimal InterestRate { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public decimal MonthlyPayment { get; set; }
    public decimal TotalPayment { get; set; }
    public decimal TotalInterest { get; set; }
    public List<PaymentPlanInstallment> Installments { get; set; } = [];
}
