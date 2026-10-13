using Ardalis.SmartEnum;

namespace Domain.Enums;

public sealed class CreditApplicationStatus : SmartEnum<CreditApplicationStatus>
{
    public static readonly CreditApplicationStatus Pending = new(nameof(Pending), 1);
    public static readonly CreditApplicationStatus Approved = new(nameof(Approved), 2);
    public static readonly CreditApplicationStatus Rejected = new(nameof(Rejected), 3);
    public static readonly CreditApplicationStatus Cancelled = new(nameof(Cancelled), 4);

    private CreditApplicationStatus(string name, int value) : base(name, value)
    {
    }
}
