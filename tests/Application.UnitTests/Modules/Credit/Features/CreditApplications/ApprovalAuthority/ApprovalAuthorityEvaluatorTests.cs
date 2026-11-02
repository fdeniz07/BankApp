using Application.Modules.Credit.Features.CreditApplications.ApprovalAuthority;
using Domain.Enums;
using FluentAssertions;

namespace Application.UnitTests.Features.CreditApplications.ApprovalAuthority;

public class ApprovalAuthorityEvaluatorTests
{
    private static ApprovalAuthorityOptions EnabledOptions() => new()
    {
        Enabled = true,
        LimitsByPosition = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
        {
            [StaffPosition.CreditAnalyst.Name] = 50_000m,
            [StaffPosition.BranchManager.Name] = 500_000m
        },
        UnlimitedRoles = ["BankManager", "SuperAdmin"]
    };

    [Fact]
    public void Resolve_WhenDisabled_ReturnsEmpty()
    {
        ApprovalAuthorityLimit limit = ApprovalAuthorityEvaluator.Resolve(
            new ApprovalAuthorityOptions { Enabled = false },
            isBankManager: false,
            isSuperAdmin: false,
            StaffPosition.CreditAnalyst,
            branchMaxApprovalAmount: 10_000m);

        limit.AppliedMax.Should().BeNull();
        limit.PositionMax.Should().BeNull();
        limit.BranchMax.Should().BeNull();
    }

    [Fact]
    public void Resolve_WhenBranchLowerThanPosition_AppliesMin()
    {
        ApprovalAuthorityLimit limit = ApprovalAuthorityEvaluator.Resolve(
            EnabledOptions(),
            isBankManager: false,
            isSuperAdmin: false,
            StaffPosition.CreditAnalyst,
            branchMaxApprovalAmount: 20_000m);

        limit.PositionMax.Should().Be(50_000m);
        limit.BranchMax.Should().Be(20_000m);
        limit.AppliedMax.Should().Be(20_000m);
    }

    [Fact]
    public void Resolve_WhenPositionLowerThanBranch_AppliesPosition()
    {
        ApprovalAuthorityLimit limit = ApprovalAuthorityEvaluator.Resolve(
            EnabledOptions(),
            isBankManager: false,
            isSuperAdmin: false,
            StaffPosition.CreditAnalyst,
            branchMaxApprovalAmount: 200_000m);

        limit.AppliedMax.Should().Be(50_000m);
        limit.BranchMax.Should().Be(200_000m);
    }

    [Fact]
    public void Resolve_WhenNoBranchCap_UsesPositionOnly()
    {
        ApprovalAuthorityLimit limit = ApprovalAuthorityEvaluator.Resolve(
            EnabledOptions(),
            isBankManager: false,
            isSuperAdmin: false,
            StaffPosition.CreditAnalyst,
            branchMaxApprovalAmount: null);

        limit.BranchMax.Should().BeNull();
        limit.AppliedMax.Should().Be(50_000m);
    }

    [Fact]
    public void Resolve_WhenUnlimitedRole_BypassesBothCaps()
    {
        ApprovalAuthorityLimit limit = ApprovalAuthorityEvaluator.Resolve(
            EnabledOptions(),
            isBankManager: true,
            isSuperAdmin: false,
            StaffPosition.CreditAnalyst,
            branchMaxApprovalAmount: 10_000m);

        limit.AppliedMax.Should().BeNull();
        limit.PositionMax.Should().BeNull();
        limit.BranchMax.Should().Be(10_000m);
    }
}
