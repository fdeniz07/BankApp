using System.Linq.Expressions;
using Application.Modules.Identity.Features.StaffUsers.Constants;
using Application.Modules.Identity.Features.StaffUsers.Rules;
using Application.Services.Repositories;
using Core.CrossCuttingConcerns.Exceptions.Types;
using Core.Security.Constants;
using Core.Security.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Moq;

namespace Application.UnitTests.Modules.Identity.Features.StaffUsers.Rules;

public class StaffUserBusinessRulesTests
{
    private readonly Mock<IApplicationUserRepository> _users = new();
    private readonly Mock<IUserOperationClaimRepository> _userClaims = new();
    private readonly Mock<IOperationClaimRepository> _claims = new();
    private readonly StaffUserBusinessRules _sut;

    private static readonly Guid ManagerClaimId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid SuperAdminClaimId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    public StaffUserBusinessRulesTests()
    {
        _sut = new StaffUserBusinessRules(_users.Object, _userClaims.Object, _claims.Object);
        SetupClaimLookup(GeneralOperationClaims.BankManager, ManagerClaimId);
        SetupClaimLookup(GeneralOperationClaims.SuperAdmin, SuperAdminClaimId);
    }

    [Fact]
    public async Task EnsureBankManagerMayMutateTarget_WhenSuperAdmin_DoesNotThrow()
    {
        Func<Task> act = () => _sut.EnsureBankManagerMayMutateTargetAsync(
            actorIsSuperAdmin: true,
            isSelf: false,
            targetUserId: Guid.NewGuid());

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task EnsureBankManagerMayMutateTarget_WhenSelf_DoesNotThrow()
    {
        Func<Task> act = () => _sut.EnsureBankManagerMayMutateTargetAsync(
            actorIsSuperAdmin: false,
            isSelf: true,
            targetUserId: Guid.NewGuid());

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task EnsureBankManagerMayMutateTarget_WhenTargetIsManager_Throws()
    {
        Guid targetId = Guid.NewGuid();
        SetupUserHasClaim(targetId, ManagerClaimId, true);

        Func<Task> act = () => _sut.EnsureBankManagerMayMutateTargetAsync(
            actorIsSuperAdmin: false,
            isSelf: false,
            targetUserId: targetId);

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage(StaffUserMessages.CannotManageOtherManagers);
    }

    [Fact]
    public async Task EnsureBankManagerMayMutateTarget_WhenTargetIsStaff_DoesNotThrow()
    {
        Guid targetId = Guid.NewGuid();
        SetupUserHasClaim(targetId, ManagerClaimId, false);
        SetupUserHasClaim(targetId, SuperAdminClaimId, false);

        Func<Task> act = () => _sut.EnsureBankManagerMayMutateTargetAsync(
            actorIsSuperAdmin: false,
            isSelf: false,
            targetUserId: targetId);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task OnlySuperAdminMayAssignManagerRole_WhenManagerTriesChange_Throws()
    {
        Func<Task> act = () => _sut.OnlySuperAdminMayAssignManagerRole(
            actorIsSuperAdmin: false,
            currentIsManager: false,
            nextIsManager: true);

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage(StaffUserMessages.OnlySuperAdminMayAssignManager);
    }

    [Fact]
    public async Task OnlySuperAdminMayAssignManagerRole_WhenUnchanged_DoesNotThrow()
    {
        Func<Task> act = () => _sut.OnlySuperAdminMayAssignManagerRole(
            actorIsSuperAdmin: false,
            currentIsManager: true,
            nextIsManager: true);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task OnlySuperAdminMayAssignManagerRole_WhenSuperAdmin_DoesNotThrow()
    {
        Func<Task> act = () => _sut.OnlySuperAdminMayAssignManagerRole(
            actorIsSuperAdmin: true,
            currentIsManager: false,
            nextIsManager: true);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task OnlySuperAdminMayCreateManager_WhenManagerRequestsIsManager_Throws()
    {
        Func<Task> act = () => _sut.OnlySuperAdminMayCreateManager(
            actorIsSuperAdmin: false,
            requestIsManager: true);

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage(StaffUserMessages.OnlySuperAdminMayCreateManager);
    }

    [Fact]
    public async Task OnlySuperAdminMayCreateManager_WhenSuperAdmin_DoesNotThrow()
    {
        Func<Task> act = () => _sut.OnlySuperAdminMayCreateManager(
            actorIsSuperAdmin: true,
            requestIsManager: true);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task OnlySuperAdminMayCreateManager_WhenStaffCreate_DoesNotThrow()
    {
        Func<Task> act = () => _sut.OnlySuperAdminMayCreateManager(
            actorIsSuperAdmin: false,
            requestIsManager: false);

        await act.Should().NotThrowAsync();
    }

    private void SetupClaimLookup(string name, Guid claimId)
    {
        _claims
            .Setup(r => r.GetAsync(
                It.Is<Expression<Func<OperationClaim, bool>>>(expr =>
                    PredicateMatchesName(expr, name)),
                It.IsAny<Func<IQueryable<OperationClaim>, IIncludableQueryable<OperationClaim, object>>>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OperationClaim { Id = claimId, Name = name });
    }

    private void SetupUserHasClaim(Guid userId, Guid claimId, bool hasClaim)
    {
        _userClaims
            .Setup(r => r.AnyAsync(
                It.Is<Expression<Func<UserOperationClaim, bool>>>(expr =>
                    PredicateMatchesUserAndClaim(expr, userId, claimId)),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(hasClaim);
    }

    private static bool PredicateMatchesName(
        Expression<Func<OperationClaim, bool>> expr,
        string name)
    {
        Func<OperationClaim, bool> compiled = expr.Compile();
        return compiled(new OperationClaim { Name = name })
            && !compiled(new OperationClaim { Name = name + "-other" });
    }

    private static bool PredicateMatchesUserAndClaim(
        Expression<Func<UserOperationClaim, bool>> expr,
        Guid userId,
        Guid claimId)
    {
        Func<UserOperationClaim, bool> compiled = expr.Compile();
        return compiled(new UserOperationClaim { UserId = userId, OperationClaimId = claimId })
            && !compiled(new UserOperationClaim { UserId = Guid.NewGuid(), OperationClaimId = claimId });
    }
}
