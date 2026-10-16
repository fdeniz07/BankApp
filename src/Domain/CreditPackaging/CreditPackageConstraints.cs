namespace Domain.CreditPackaging;

public static class CreditPackageConstraints
{
    // Short/medium products: up to 5 years. Housing mortgages: 10–30 years.
    public static readonly int[] AllowedTermMonths =
    [
        3, 6, 9, 12, 15, 18, 24, 36, 48, 60,
        120, 180, 240, 300, 360
    ];

    public static decimal ResolveStep(string currencyCode) =>
        string.Equals(currencyCode, "TRY", StringComparison.OrdinalIgnoreCase) ? 10_000m : 5_000m;

    public static int[] GetAllowedTermsInRange(int minTerm, int maxTerm) =>
        AllowedTermMonths.Where(t => t >= minTerm && t <= maxTerm).ToArray();

    public static bool IsAllowedTerm(int term, int minTerm, int maxTerm) =>
        GetAllowedTermsInRange(minTerm, maxTerm).Contains(term);

    public static int SnapTermMonths(int term, int minTerm, int maxTerm)
    {
        int[] candidates = GetAllowedTermsInRange(minTerm, maxTerm);
        if (candidates.Length == 0)
            return Math.Clamp(term, minTerm, maxTerm);

        return candidates.OrderBy(t => Math.Abs(t - term)).ThenBy(t => t).First();
    }

    public static decimal RoundAmount(decimal amount, string currencyCode)
    {
        decimal step = ResolveStep(currencyCode);
        if (amount <= 0)
            return step;

        decimal rounded = Math.Round(amount / step, MidpointRounding.AwayFromZero) * step;
        return Math.Max(step, rounded);
    }

    public static bool IsAllowedAmount(decimal amount, decimal min, decimal max, string currencyCode)
    {
        if (amount < min || amount > max)
            return false;

        decimal step = ResolveStep(currencyCode);
        decimal normalized = amount / step;
        return normalized == Math.Truncate(normalized);
    }

    public static IReadOnlyList<decimal> GetAmountOptions(decimal min, decimal max, string currencyCode)
    {
        decimal step = ResolveStep(currencyCode);
        decimal lo = Math.Ceiling(min / step) * step;
        decimal hi = Math.Floor(max / step) * step;
        if (hi < lo)
            return [RoundAmount(min, currencyCode)];

        List<decimal> options = [];
        for (decimal value = lo; value <= hi; value += step)
            options.Add(value);

        return options;
    }

    public static decimal PickRoundAmount(Random random, decimal min, decimal max, string currencyCode)
    {
        IReadOnlyList<decimal> options = GetAmountOptions(min, max, currencyCode);
        return options[random.Next(options.Count)];
    }
}
