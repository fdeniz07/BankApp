namespace Application.Services.Search;

/// <summary>Fixed lab histogram edges for requestedAmount (ES + SQL facets).</summary>
public static class CreditApplicationAmountBuckets
{
    public static readonly (decimal From, decimal? To)[] Ranges =
    [
        (0m, 50_000m),
        (50_000m, 100_000m),
        (100_000m, 250_000m),
        (250_000m, 500_000m),
        (500_000m, null)
    ];

    public static int BucketIndex(decimal amount)
    {
        for (int i = 0; i < Ranges.Length; i++)
        {
            (decimal from, decimal? to) = Ranges[i];
            if (amount >= from && (to is null || amount < to.Value))
                return i;
        }

        return Ranges.Length - 1;
    }
}
