using Ardalis.SmartEnum;

namespace Domain.Enums;

public sealed class CreditRiskLevel : SmartEnum<CreditRiskLevel>
{
    public static readonly CreditRiskLevel Low = new(nameof(Low), 1);
    public static readonly CreditRiskLevel Medium = new(nameof(Medium), 2);
    public static readonly CreditRiskLevel High = new(nameof(High), 3);

    private CreditRiskLevel(string name, int value) : base(name, value)
    {
    }
}
