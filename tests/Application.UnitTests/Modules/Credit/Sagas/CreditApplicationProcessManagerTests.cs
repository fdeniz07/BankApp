using System.Linq.Expressions;
using System.Text.Json;
using Application.Modules.Credit.Sagas;
using Application.Services.Repositories;
using Domain.Entities;
using Domain.Enums;
using Domain.Events;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Application.UnitTests.Sagas;

public class CreditApplicationProcessManagerTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task Created_twice_adds_saga_only_once()
    {
        Guid applicationId = Guid.NewGuid();
        var created = new CreditApplicationCreated(
            applicationId,
            "APP-PM-1",
            Guid.NewGuid(),
            Guid.NewGuid(),
            10_000m,
            12,
            DateTimeOffset.UtcNow);

        CreditApplicationSaga? stored = null;
        var repo = new Mock<ICreditApplicationSagaRepository>();
        repo.Setup(r => r.GetAsync(
                It.IsAny<Expression<Func<CreditApplicationSaga, bool>>>(),
                It.IsAny<Func<IQueryable<CreditApplicationSaga>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<CreditApplicationSaga, object>>>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => stored);
        repo.Setup(r => r.AddAsync(It.IsAny<CreditApplicationSaga>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CreditApplicationSaga s, CancellationToken _) =>
            {
                stored = s;
                return s;
            });

        CreditApplicationProcessManager manager = new(repo.Object, NullLogger<CreditApplicationProcessManager>.Instance);
        string payload = JsonSerializer.Serialize(created, JsonOptions);
        string type = typeof(CreditApplicationCreated).FullName!;

        await manager.HandleAsync(type, payload);
        await manager.HandleAsync(type, payload);

        repo.Verify(r => r.AddAsync(It.IsAny<CreditApplicationSaga>(), It.IsAny<CancellationToken>()), Times.Once);
        stored.Should().NotBeNull();
        stored!.Status.Should().Be(CreditApplicationSagaStatus.AwaitingDecision);
    }

    [Fact]
    public async Task Approved_runs_onboarding_to_Completed()
    {
        Guid applicationId = Guid.NewGuid();
        CreditApplicationSaga saga = CreditApplicationSaga.StartFromCreated(applicationId, "APP-PM-2");

        var repo = new Mock<ICreditApplicationSagaRepository>();
        repo.Setup(r => r.GetAsync(
                It.IsAny<Expression<Func<CreditApplicationSaga, bool>>>(),
                It.IsAny<Func<IQueryable<CreditApplicationSaga>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<CreditApplicationSaga, object>>>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(saga);
        repo.Setup(r => r.UpdateAsync(It.IsAny<CreditApplicationSaga>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CreditApplicationSaga s, CancellationToken _) => s);

        var approved = new CreditApplicationApproved(
            applicationId,
            "APP-PM-2",
            Guid.NewGuid(),
            10_000m,
            12,
            1.5m,
            "staff",
            DateTimeOffset.UtcNow);

        CreditApplicationProcessManager manager = new(repo.Object, NullLogger<CreditApplicationProcessManager>.Instance);
        await manager.HandleAsync(
            typeof(CreditApplicationApproved).FullName!,
            JsonSerializer.Serialize(approved, JsonOptions));

        saga.Status.Should().Be(CreditApplicationSagaStatus.Completed);
        saga.CurrentStep.Should().Be(CreditApplicationSagaStep.Done);
        saga.ContractReference.Should().NotBeNullOrWhiteSpace();
        repo.Verify(r => r.UpdateAsync(It.IsAny<CreditApplicationSaga>(), It.IsAny<CancellationToken>()), Times.AtLeast(3));
    }

    [Fact]
    public async Task Rejected_terminates_AwaitingDecision_saga()
    {
        Guid applicationId = Guid.NewGuid();
        CreditApplicationSaga saga = CreditApplicationSaga.StartFromCreated(applicationId, "APP-PM-3");

        var repo = new Mock<ICreditApplicationSagaRepository>();
        repo.Setup(r => r.GetAsync(
                It.IsAny<Expression<Func<CreditApplicationSaga, bool>>>(),
                It.IsAny<Func<IQueryable<CreditApplicationSaga>, Microsoft.EntityFrameworkCore.Query.IIncludableQueryable<CreditApplicationSaga, object>>>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(saga);
        repo.Setup(r => r.UpdateAsync(It.IsAny<CreditApplicationSaga>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CreditApplicationSaga s, CancellationToken _) => s);

        var rejected = new CreditApplicationRejected(
            applicationId,
            "APP-PM-3",
            Guid.NewGuid(),
            "insufficient income",
            "staff",
            DateTimeOffset.UtcNow);

        CreditApplicationProcessManager manager = new(repo.Object, NullLogger<CreditApplicationProcessManager>.Instance);
        await manager.HandleAsync(
            typeof(CreditApplicationRejected).FullName!,
            JsonSerializer.Serialize(rejected, JsonOptions));

        saga.Status.Should().Be(CreditApplicationSagaStatus.Terminated);
        saga.FailureReason.Should().Contain("insufficient income");
    }
}
