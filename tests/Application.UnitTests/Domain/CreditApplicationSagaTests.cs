using Domain.Entities;
using Domain.Enums;
using FluentAssertions;

namespace Application.UnitTests.Domain;

public class CreditApplicationSagaTests
{
    [Fact]
    public void StartFromCreated_sets_AwaitingDecision()
    {
        Guid applicationId = Guid.NewGuid();

        CreditApplicationSaga saga = CreditApplicationSaga.StartFromCreated(applicationId, "APP-SAGA-1");

        saga.CreditApplicationId.Should().Be(applicationId);
        saga.ApplicationNumber.Should().Be("APP-SAGA-1");
        saga.Status.Should().Be(CreditApplicationSagaStatus.AwaitingDecision);
        saga.CurrentStep.Should().Be(CreditApplicationSagaStep.None);
        saga.ContractReference.Should().BeNull();
    }

    [Fact]
    public void Onboarding_happy_path_reaches_Completed()
    {
        CreditApplicationSaga saga = CreditApplicationSaga.StartFromCreated(Guid.NewGuid(), "APP-SAGA-2");

        saga.BeginOnboarding();
        saga.MarkRiskChecked();
        saga.MarkContractPrepared("CTR-APP-SAGA-2");
        saga.MarkCustomerNotified();
        saga.Complete();

        saga.Status.Should().Be(CreditApplicationSagaStatus.Completed);
        saga.CurrentStep.Should().Be(CreditApplicationSagaStep.Done);
        saga.ContractReference.Should().Be("CTR-APP-SAGA-2");
        saga.CompletedOnUtc.Should().NotBeNull();
    }

    [Fact]
    public void Terminate_from_AwaitingDecision_sets_Terminated()
    {
        CreditApplicationSaga saga = CreditApplicationSaga.StartFromCreated(Guid.NewGuid(), "APP-SAGA-3");

        saga.Terminate("Rejected: low score");

        saga.Status.Should().Be(CreditApplicationSagaStatus.Terminated);
        saga.FailureReason.Should().Be("Rejected: low score");
        saga.CompletedOnUtc.Should().NotBeNull();
    }

    [Fact]
    public void Compensate_from_Onboarding_clears_contract()
    {
        CreditApplicationSaga saga = CreditApplicationSaga.StartFromCreated(Guid.NewGuid(), "APP-SAGA-4");
        saga.BeginOnboarding();
        saga.MarkRiskChecked();
        saga.MarkContractPrepared("CTR-TEMP");

        saga.Compensate("step failure");

        saga.Status.Should().Be(CreditApplicationSagaStatus.Compensated);
        saga.ContractReference.Should().BeNull();
        saga.FailureReason.Should().Be("step failure");
    }

    [Fact]
    public void Terminate_from_Onboarding_throws()
    {
        CreditApplicationSaga saga = CreditApplicationSaga.StartFromCreated(Guid.NewGuid(), "APP-SAGA-5");
        saga.BeginOnboarding();

        Action act = () => saga.Terminate("too late");

        act.Should().Throw<InvalidOperationException>();
    }
}
