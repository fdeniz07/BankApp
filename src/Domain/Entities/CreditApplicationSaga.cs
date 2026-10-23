using Core.Repositories;
using Domain.Enums;

namespace Domain.Entities;

public class CreditApplicationSaga : Entity
{
    public Guid CreditApplicationId { get; set; }
    public string ApplicationNumber { get; set; } = default!;
    public CreditApplicationSagaStatus Status { get; set; } = CreditApplicationSagaStatus.AwaitingDecision;
    public CreditApplicationSagaStep CurrentStep { get; set; } = CreditApplicationSagaStep.None;
    public string? ContractReference { get; set; }
    public string? FailureReason { get; set; }
    public DateTimeOffset StartedOnUtc { get; set; }
    public DateTimeOffset? CompletedOnUtc { get; set; }

    public static CreditApplicationSaga StartFromCreated(Guid creditApplicationId, string applicationNumber)
    {
        return new CreditApplicationSaga
        {
            CreditApplicationId = creditApplicationId,
            ApplicationNumber = applicationNumber,
            Status = CreditApplicationSagaStatus.AwaitingDecision,
            CurrentStep = CreditApplicationSagaStep.None,
            StartedOnUtc = DateTimeOffset.UtcNow
        };
    }

    public void BeginOnboarding()
    {
        if (Status != CreditApplicationSagaStatus.AwaitingDecision)
            throw new InvalidOperationException(
                $"Saga {CreditApplicationId} cannot begin onboarding from status {Status.Name}.");

        Status = CreditApplicationSagaStatus.Onboarding;
        UpdatedDate = DateTimeOffset.UtcNow;
    }

    public void MarkRiskChecked()
    {
        EnsureOnboarding();
        CurrentStep = CreditApplicationSagaStep.RiskChecked;
        UpdatedDate = DateTimeOffset.UtcNow;
    }

    public void MarkContractPrepared(string contractReference)
    {
        EnsureOnboarding();
        ArgumentException.ThrowIfNullOrWhiteSpace(contractReference);
        ContractReference = contractReference;
        CurrentStep = CreditApplicationSagaStep.ContractPrepared;
        UpdatedDate = DateTimeOffset.UtcNow;
    }

    public void MarkCustomerNotified()
    {
        EnsureOnboarding();
        CurrentStep = CreditApplicationSagaStep.CustomerNotified;
        UpdatedDate = DateTimeOffset.UtcNow;
    }

    public void Complete()
    {
        EnsureOnboarding();
        CurrentStep = CreditApplicationSagaStep.Done;
        Status = CreditApplicationSagaStatus.Completed;
        CompletedOnUtc = DateTimeOffset.UtcNow;
        UpdatedDate = CompletedOnUtc;
    }

    public void Terminate(string reason)
    {
        if (Status == CreditApplicationSagaStatus.Completed
            || Status == CreditApplicationSagaStatus.Terminated
            || Status == CreditApplicationSagaStatus.Compensated)
            throw new InvalidOperationException(
                $"Saga {CreditApplicationId} is already terminal ({Status.Name}).");

        if (Status != CreditApplicationSagaStatus.AwaitingDecision)
            throw new InvalidOperationException(
                $"Saga {CreditApplicationId} can only terminate from AwaitingDecision, was {Status.Name}.");

        Status = CreditApplicationSagaStatus.Terminated;
        FailureReason = reason;
        CompletedOnUtc = DateTimeOffset.UtcNow;
        UpdatedDate = CompletedOnUtc;
    }

    public void Compensate(string reason)
    {
        if (Status != CreditApplicationSagaStatus.Onboarding)
            throw new InvalidOperationException(
                $"Saga {CreditApplicationId} can only compensate from Onboarding, was {Status.Name}.");

        ContractReference = null;
        Status = CreditApplicationSagaStatus.Compensated;
        FailureReason = reason;
        CompletedOnUtc = DateTimeOffset.UtcNow;
        UpdatedDate = CompletedOnUtc;
    }

    private void EnsureOnboarding()
    {
        if (Status != CreditApplicationSagaStatus.Onboarding)
            throw new InvalidOperationException(
                $"Saga {CreditApplicationId} is not in Onboarding (status={Status.Name}).");
    }
}
