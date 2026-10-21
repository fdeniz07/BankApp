using Domain.Entities;
using Domain.Enums;
using Domain.Events;
using FluentAssertions;

namespace Application.UnitTests.Domain;

public class CreditApplicationDomainEventsTests
{
    [Fact]
    public void InitializeAsNewApplication_raises_Created_event()
    {
        CreditApplication application = new()
        {
            CustomerId = Guid.NewGuid(),
            CreditSubTypeId = Guid.NewGuid(),
            RequestedAmount = 25_000m,
            RequestedTermMonths = 12
        };

        application.InitializeAsNewApplication("APP-TEST-001");

        application.Status.Should().Be(CreditApplicationStatus.Pending);
        application.ApplicationNumber.Should().Be("APP-TEST-001");
        application.GetDomainEvents().Should().ContainSingle()
            .Which.Should().BeOfType<CreditApplicationCreated>();
    }

    [Fact]
    public void Approve_raises_Approved_event()
    {
        CreditApplication application = CreatePending();

        application.Approve(40_000m, 24, 2.5m, "staff", "ok");

        application.Status.Should().Be(CreditApplicationStatus.Approved);
        application.GetDomainEvents().Should().ContainSingle()
            .Which.Should().BeOfType<CreditApplicationApproved>();
    }

    [Fact]
    public void Reject_raises_Rejected_event()
    {
        CreditApplication application = CreatePending();

        application.Reject("insufficient income", "staff", null);

        application.Status.Should().Be(CreditApplicationStatus.Rejected);
        application.GetDomainEvents().Should().ContainSingle()
            .Which.Should().BeOfType<CreditApplicationRejected>();
    }

    [Fact]
    public void Cancel_raises_Cancelled_event()
    {
        CreditApplication application = CreatePending();

        application.Cancel("customer request");

        application.Status.Should().Be(CreditApplicationStatus.Cancelled);
        application.GetDomainEvents().Should().ContainSingle()
            .Which.Should().BeOfType<CreditApplicationCancelled>();
    }

    private static CreditApplication CreatePending()
    {
        CreditApplication application = new()
        {
            CustomerId = Guid.NewGuid(),
            CreditSubTypeId = Guid.NewGuid(),
            RequestedAmount = 25_000m,
            RequestedTermMonths = 12
        };
        application.InitializeAsNewApplication("APP-TEST-002");
        application.ClearDomainEvents();
        return application;
    }
}
