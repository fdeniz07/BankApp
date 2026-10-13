using Ardalis.SmartEnum;

namespace Domain.Enums;

public sealed class CreditCategory : SmartEnum<CreditCategory>
{
    public static readonly CreditCategory Individual = new(nameof(Individual), 1);
    public static readonly CreditCategory Corporate = new(nameof(Corporate), 2);

    private CreditCategory(string name, int value) : base(name, value)
    {
    }
}
