using Ardalis.SmartEnum;

namespace Domain.Enums;

public sealed class CreditApplicationSagaStep : SmartEnum<CreditApplicationSagaStep>
{
    public static readonly CreditApplicationSagaStep None = new(nameof(None), 0);
    public static readonly CreditApplicationSagaStep RiskChecked = new(nameof(RiskChecked), 1);
    public static readonly CreditApplicationSagaStep ContractPrepared = new(nameof(ContractPrepared), 2);
    public static readonly CreditApplicationSagaStep CustomerNotified = new(nameof(CustomerNotified), 3);
    public static readonly CreditApplicationSagaStep Done = new(nameof(Done), 4);

    private CreditApplicationSagaStep(string name, int value) : base(name, value)
    {
    }
}
