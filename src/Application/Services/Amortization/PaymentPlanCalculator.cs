namespace Application.Services.Amortization;

public static class PaymentPlanCalculator
{
    public static IReadOnlyList<PaymentPlanInstallment> Build(
        decimal principal,
        int termMonths,
        decimal annualInterestRatePercent)
    {
        if (principal <= 0 || termMonths <= 0)
            return [];

        decimal monthlyRate = annualInterestRatePercent / 100m / 12m;
        decimal payment = monthlyRate == 0
            ? Math.Round(principal / termMonths, 2, MidpointRounding.AwayFromZero)
            : Math.Round(
                principal * monthlyRate * Pow(1 + monthlyRate, termMonths)
                / (Pow(1 + monthlyRate, termMonths) - 1),
                2,
                MidpointRounding.AwayFromZero);

        decimal balance = principal;
        List<PaymentPlanInstallment> rows = [];

        for (int month = 1; month <= termMonths; month++)
        {
            decimal interest = Math.Round(balance * monthlyRate, 2, MidpointRounding.AwayFromZero);
            decimal principalPart = month == termMonths
                ? balance
                : Math.Round(payment - interest, 2, MidpointRounding.AwayFromZero);

            if (principalPart > balance)
                principalPart = balance;

            decimal installment = principalPart + interest;
            balance = Math.Round(balance - principalPart, 2, MidpointRounding.AwayFromZero);
            if (balance < 0.01m)
                balance = 0;

            rows.Add(new PaymentPlanInstallment
            {
                Month = month,
                Payment = installment,
                Principal = principalPart,
                Interest = interest,
                RemainingBalance = balance
            });
        }

        return rows;
    }

    private static decimal Pow(decimal value, int exponent)
    {
        decimal result = 1m;
        for (int i = 0; i < exponent; i++)
            result *= value;
        return result;
    }
}

public class PaymentPlanInstallment
{
    public int Month { get; set; }
    public decimal Payment { get; set; }
    public decimal Principal { get; set; }
    public decimal Interest { get; set; }
    public decimal RemainingBalance { get; set; }
}
