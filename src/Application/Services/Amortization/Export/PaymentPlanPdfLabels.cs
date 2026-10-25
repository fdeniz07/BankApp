using System.Globalization;

namespace Application.Services.Amortization.Export;

internal sealed record PaymentPlanPdfLabels(
    string Title,
    string ApplicationNumber,
    string Customer,
    string Amount,
    string Term,
    string MonthsUnit,
    string Rate,
    string Monthly,
    string Total,
    string TotalInterest,
    string GeneratedUtc,
    string Month,
    string Installment,
    string Principal,
    string Interest,
    string Balance,
    string Page,
    CultureInfo Culture)
{
    public static PaymentPlanPdfLabels For(string? locale)
    {
        string lang = (locale ?? "en").Trim().ToLowerInvariant();
        if (lang.StartsWith("tr", StringComparison.Ordinal))
            return Turkish();
        if (lang.StartsWith("de", StringComparison.Ordinal))
            return German();
        return English();
    }

    private static PaymentPlanPdfLabels Turkish() => new(
        Title: "Ödeme planı",
        ApplicationNumber: "Kredi numarası",
        Customer: "Müşteri",
        Amount: "Tutar",
        Term: "Vade",
        MonthsUnit: "ay",
        Rate: "Faiz",
        Monthly: "Aylık taksit",
        Total: "Toplam",
        TotalInterest: "Toplam faiz",
        GeneratedUtc: "Oluşturulma (UTC)",
        Month: "Ay",
        Installment: "Taksit",
        Principal: "Ana para",
        Interest: "Faiz",
        Balance: "Kalan bakiye",
        Page: "Sayfa",
        Culture: CultureInfo.GetCultureInfo("tr-TR"));

    private static PaymentPlanPdfLabels English() => new(
        Title: "Payment schedule",
        ApplicationNumber: "Application number",
        Customer: "Customer",
        Amount: "Amount",
        Term: "Term",
        MonthsUnit: "months",
        Rate: "Rate",
        Monthly: "Monthly installment",
        Total: "Total",
        TotalInterest: "Total interest",
        GeneratedUtc: "Generated (UTC)",
        Month: "Month",
        Installment: "Installment",
        Principal: "Principal",
        Interest: "Interest",
        Balance: "Remaining balance",
        Page: "Page",
        Culture: CultureInfo.GetCultureInfo("en-US"));

    private static PaymentPlanPdfLabels German() => new(
        Title: "Zahlungsplan",
        ApplicationNumber: "Kreditantragsnummer",
        Customer: "Kunde",
        Amount: "Betrag",
        Term: "Laufzeit",
        MonthsUnit: "Monate",
        Rate: "Zinssatz",
        Monthly: "Monatsrate",
        Total: "Gesamt",
        TotalInterest: "Gesamtzinsen",
        GeneratedUtc: "Erstellt (UTC)",
        Month: "Monat",
        Installment: "Rate",
        Principal: "Tilgung",
        Interest: "Zinsen",
        Balance: "Restschuld",
        Page: "Seite",
        Culture: CultureInfo.GetCultureInfo("de-DE"));
}
