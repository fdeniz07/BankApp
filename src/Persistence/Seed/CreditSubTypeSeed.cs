using Domain.CreditPackaging;
using Domain.Entities;
using Domain.Enums;

namespace Persistence.Seed;

public static class CreditSubTypeSeed
{
    public const int SubTypesPerCreditType = 2;

    public static readonly int[] AllowedTermMonths = CreditPackageConstraints.AllowedTermMonths;

    private static readonly Dictionary<string, (string[] Individual, string[] Corporate)> SuffixesByLocale = new()
    {
        ["tr"] = (["Standart", "Avantajlı"], ["Kısa Vade", "Uzun Vade"]),
        ["en"] = (["Standard", "Preferred"], ["Short Term", "Long Term"]),
        ["de"] = (["Standard", "Vorteilsangebot"], ["Kurzfristig", "Langfristig"])
    };

    private static readonly Dictionary<string, string> DescriptionTemplates = new()
    {
        ["tr"] = "{0} için {1} paket",
        ["en"] = "{1} package for {0}",
        ["de"] = "{1}-Paket für {0}"
    };

    public static string ResolveLocale(CreditType creditType) =>
        string.IsNullOrWhiteSpace(creditType.Locale)
            ? ResolveLocaleFromCode(creditType.Code)
            : creditType.Locale.ToLowerInvariant();

    public static string ResolveLocaleFromCode(string creditTypeCode)
    {
        if (creditTypeCode.Contains("-EN-", StringComparison.OrdinalIgnoreCase) ||
            creditTypeCode.StartsWith("CT-EN-", StringComparison.OrdinalIgnoreCase))
            return "en";
        if (creditTypeCode.Contains("-DE-", StringComparison.OrdinalIgnoreCase) ||
            creditTypeCode.StartsWith("CT-DE-", StringComparison.OrdinalIgnoreCase))
            return "de";
        return "tr";
    }

    public static List<CreditSubType> GenerateForCreditType(CreditType creditType, int count, int startIndex = 0)
    {
        if (count <= 0)
            return [];

        string locale = ResolveLocale(creditType);
        bool isCorporate = creditType.Category == CreditCategory.Corporate;
        string[] suffixes = isCorporate
            ? SuffixesByLocale[locale].Corporate
            : SuffixesByLocale[locale].Individual;

        int sequence = startIndex;
        List<CreditSubType> items = [];

        for (int i = 0; i < count; i++)
        {
            int index = startIndex + i;
            string suffix = suffixes[Math.Min(index, suffixes.Length - 1)];
            bool isLongPackage = index % 2 == 1;

            (decimal minAmount, decimal maxAmount) = ResolveAmountRange(creditType.CurrencyCode, isCorporate);
            (int minTerm, int maxTerm) = ResolveTermRange(creditType, isLongPackage);
            decimal interest = ResolveInterestRate(creditType, isLongPackage);

            items.Add(new CreditSubType
            {
                CreditTypeId = creditType.Id,
                Name = $"{creditType.Name} {suffix}",
                Code = $"{creditType.Code}-ST-{(++sequence):D2}",
                Description = string.Format(DescriptionTemplates[locale], creditType.Name, suffix),
                MinAmount = minAmount,
                MaxAmount = maxAmount,
                InterestRate = interest,
                MinTermMonths = minTerm,
                MaxTermMonths = maxTerm,
                LateFeeRate = 0.5m,
                EarlyPaymentFeeRate = 1.0m,
                ProcessingFeeAmount = CreditPackageConstraints.RoundAmount(minAmount * 0.01m, creditType.CurrencyCode),
                ProcessingFeeRate = 0.5m,
                GracePeriodDays = 15,
                RequiresCollateral = creditType.RequiresCollateral,
                RequiresIncomeDocument = !isCorporate,
                IsActive = true
            });
        }

        return items;
    }

    public static bool ApplyCatalogValues(CreditSubType subType, CreditType creditType, int packageIndex)
    {
        string locale = ResolveLocale(creditType);
        bool isCorporate = creditType.Category == CreditCategory.Corporate;
        string[] suffixes = isCorporate
            ? SuffixesByLocale[locale].Corporate
            : SuffixesByLocale[locale].Individual;

        string suffix = suffixes[Math.Min(packageIndex, suffixes.Length - 1)];
        bool isLongPackage = packageIndex % 2 == 1;
        (decimal minAmount, decimal maxAmount) = ResolveAmountRange(creditType.CurrencyCode, isCorporate);
        (int minTerm, int maxTerm) = ResolveTermRange(creditType, isLongPackage);
        string name = $"{creditType.Name} {suffix}";
        string description = string.Format(DescriptionTemplates[locale], creditType.Name, suffix);
        decimal processingFee = CreditPackageConstraints.RoundAmount(minAmount * 0.01m, creditType.CurrencyCode);
        decimal interest = ResolveInterestRate(creditType, isLongPackage);

        bool changed =
            !string.Equals(subType.Name, name, StringComparison.Ordinal) ||
            !string.Equals(subType.Description, description, StringComparison.Ordinal) ||
            subType.MinAmount != minAmount ||
            subType.MaxAmount != maxAmount ||
            subType.MinTermMonths != minTerm ||
            subType.MaxTermMonths != maxTerm ||
            subType.InterestRate != interest ||
            subType.ProcessingFeeAmount != processingFee;

        if (!changed)
            return false;

        subType.Name = name;
        subType.Description = description;
        subType.MinAmount = minAmount;
        subType.MaxAmount = maxAmount;
        subType.MinTermMonths = minTerm;
        subType.MaxTermMonths = maxTerm;
        subType.InterestRate = interest;
        subType.ProcessingFeeAmount = processingFee;
        subType.LateFeeRate = 0.5m;
        subType.EarlyPaymentFeeRate = 1.0m;
        subType.ProcessingFeeRate = 0.5m;
        subType.GracePeriodDays = 15;
        return true;
    }

    public static int SnapTermMonths(int term, int minTerm, int maxTerm) =>
        CreditPackageConstraints.SnapTermMonths(term, minTerm, maxTerm);

    public static decimal RoundAmount(decimal amount, string currencyCode) =>
        CreditPackageConstraints.RoundAmount(amount, currencyCode);

    public static decimal PickRoundAmount(Random random, decimal min, decimal max, string currencyCode) =>
        CreditPackageConstraints.PickRoundAmount(random, min, max, currencyCode);

    public static int[] GetAllowedTermsInRange(int minTerm, int maxTerm) =>
        CreditPackageConstraints.GetAllowedTermsInRange(minTerm, maxTerm);

    private static (decimal Min, decimal Max) ResolveAmountRange(string currencyCode, bool isCorporate)
    {
        string currency = (currencyCode ?? "TRY").ToUpperInvariant();
        return (currency, isCorporate) switch
        {
            ("TRY", false) => (50_000m, 500_000m),
            ("TRY", true) => (100_000m, 5_000_000m),
            ("EUR", false) => (10_000m, 250_000m),
            ("EUR", true) => (50_000m, 2_500_000m),
            ("USD", false) => (10_000m, 250_000m),
            ("USD", true) => (50_000m, 2_500_000m),
            (_, false) => (10_000m, 250_000m),
            (_, true) => (50_000m, 2_500_000m)
        };
    }

    private static (int Min, int Max) ResolveTermRange(CreditType creditType, bool isLongPackage)
    {
        if (IsHousingCreditType(creditType))
            return (120, 360); // 10–30 years

        return isLongPackage ? (12, 60) : (3, 24);
    }

    private static decimal ResolveInterestRate(CreditType creditType, bool isLongPackage)
    {
        if (IsHousingCreditType(creditType))
            return isLongPackage ? 1.85m : 1.55m;

        return isLongPackage ? 2.25m : 1.75m;
    }

    private static bool IsHousingCreditType(CreditType creditType) =>
        creditType.Code.Contains("-IND-002", StringComparison.OrdinalIgnoreCase);
}
