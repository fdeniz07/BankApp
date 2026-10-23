using Ardalis.SmartEnum;

namespace Domain.Enums;

public sealed class CreditApplicationSagaStatus : SmartEnum<CreditApplicationSagaStatus>
{
    public static readonly CreditApplicationSagaStatus AwaitingDecision = new(nameof(AwaitingDecision), 1);
    public static readonly CreditApplicationSagaStatus Onboarding = new(nameof(Onboarding), 2);
    public static readonly CreditApplicationSagaStatus Completed = new(nameof(Completed), 3);
    public static readonly CreditApplicationSagaStatus Terminated = new(nameof(Terminated), 4);
    public static readonly CreditApplicationSagaStatus Compensated = new(nameof(Compensated), 5);

    private CreditApplicationSagaStatus(string name, int value) : base(name, value)
    {
    }
}
