using System.Globalization;

namespace Application.Services.Amortization.Export;

internal static class PaymentPlanMoneyFormatter
{
    /// <summary>
    /// Locale-aware amount + currency symbol (TRY → ₺, not the ISO code).
    /// </summary>
    public static string Format(decimal value, string currencyCode, CultureInfo culture)
    {
        string amount = value.ToString("N2", culture);
        string symbol = ResolveSymbol(currencyCode);
        return $"{amount} {symbol}";
    }

    private static string ResolveSymbol(string? currencyCode) =>
        (currencyCode ?? string.Empty).Trim().ToUpperInvariant() switch
        {
            "TRY" => "₺",
            "EUR" => "€",
            "USD" => "$",
            "GBP" => "£",
            "CHF" => "CHF",
            _ => string.IsNullOrWhiteSpace(currencyCode) ? "" : currencyCode.Trim().ToUpperInvariant()
        };
}
