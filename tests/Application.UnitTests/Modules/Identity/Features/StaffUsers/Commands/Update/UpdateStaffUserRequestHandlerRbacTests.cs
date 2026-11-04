using System.Linq.Expressions;
using Application.Modules.Identity.Features.Auth.Rules;
using Application.Modules.Identity.Features.StaffUsers.Commands.Update;
using Application.Modules.Identity.Features.StaffUsers.Constants;
using Application.Modules.Identity.Features.StaffUsers.Rules;
using Application.Services.Auth;
using Application.Services.Logging;
using Application.Services.Repositories;
using Application.Services.Search;
using Core.CrossCuttingConcerns.Exceptions.Types;
using Core.Security.Constants;
using Core.Security.Entities;
using Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Moq;

namespace Application.UnitTests.Modules.Identity.Features.StaffUsers.Commands.Update;

public class UpdateStaffUserRequestHandlerRbacTests
{
    private readonly Mock<IApplicationUserRepository> _users = new();
    private readonly Mock<ICustomerRepository> _customers = new();
    private readonly Mock<IOperationClaimRepository> _claims = new();
    private readonly Mock<IUserOperationClaimRepository> _userClaims = new();
    private readonly Mock<IBranchRepository> _branches = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly Mock<IBranchAuthorizationService> _branchAuth = new();
    private readonly Mock<IAuditLogger> _audit = new();
    private readonly Mock<IStaffUserSearchService> _staffSearch = new();

    private static readonly Guid ManagerClaimId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid SuperAdminClaimId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    public UpdateStaffUserRequestHandlerRbacTests()
    {
        SetupClaimLookup(GeneralOperationClaims.BankManager, ManagerClaimId);
        SetupClaimLookup(GeneralOperationClaims.SuperAdmin, SuperAdminClaimId);
        _branchAuth
            .Setup(s => s.EnsureCanManageStaffUserAsync(It.IsAny<ApplicationUser>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _staffSearch
            .Setup(s => s.UpsertFromUserAsync(It.IsAny<ApplicationUser>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task Handle_WhenManagerUpdatesOtherManager_Throws()
    {
        Guid actorId = Guid.NewGuid();
        Guid targetId = Guid.NewGuid();
        ApplicationUser target = StaffUser(targetId, "manager2@bank.local");

        SetupActor(actorId, isBankManager: true, isSuperAdmin: false);
        SetupGetUser(target);
        SetupUserHasClaim(targetId, ManagerClaimId, true);

        UpdateStaffUserRequest request = BaseUpdate(targetId, target.Email);

        Func<Task> act = () => CreateSut().Handle(request, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage(StaffUserMessages.CannotManageOtherManagers);
    }

    [Fact]
    public async Task Handle_WhenManagerUpdatesStaff_Succeeds()
    {
        Guid actorId = Guid.NewGuid();
        Guid targetId = Guid.NewGuid();
        Guid branchId = Guid.NewGuid();
        ApplicationUser target = StaffUser(targetId, "staff@bank.local", branchId);

        SetupActor(actorId, isBankManager: true, isSuperAdmin: false, branchId: branchId);
        SetupGetUser(target);
        SetupUserHasClaim(targetId, ManagerClaimId, false);
        SetupUserHasClaim(targetId, SuperAdminClaimId, false);
        SetupBranch(branchId);
        SetupUpdateAndClaims(target, isManager: false);

        UpdateStaffUserRequest request = BaseUpdate(targetId, "staff.updated@bank.local");
        request.IsActive = true;
        request.IsManager = false;
        request.BranchId = branchId;
        request.FirstName = "Ada";

        UpdatedStaffUserResponse response = await CreateSut().Handle(request, CancellationToken.None);

        response.Email.Should().Be("staff.updated@bank.local");
        response.FirstName.Should().Be("Ada");
        response.IsActive.Should().BeTrue();
        response.IsManager.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenSuperAdminUpdatesManager_Succeeds()
    {
        Guid actorId = Guid.NewGuid();
        Guid targetId = Guid.NewGuid();
        Guid branchId = Guid.NewGuid();
        ApplicationUser target = StaffUser(targetId, "manager@bank.local", branchId);

        SetupActor(actorId, isBankManager: true, isSuperAdmin: true, branchId: branchId);
        SetupGetUser(target);
        SetupUserHasClaim(targetId, ManagerClaimId, true);
        SetupBranch(branchId);
        SetupUpdateAndClaims(target, isManager: true);
        SetupUserClaimForSync(targetId, ManagerClaimId, exists: true);

        UpdateStaffUserRequest request = BaseUpdate(targetId, "manager.updated@bank.local");
        request.IsActive = true;
        request.IsManager = true;
        request.BranchId = branchId;
        request.FirstName = "Grace";

        UpdatedStaffUserResponse response = await CreateSut().Handle(request, CancellationToken.None);

        response.Email.Should().Be("manager.updated@bank.local");
        response.FirstName.Should().Be("Grace");
        response.IsManager.Should().BeTrue();
    }

    private UpdateStaffUserRequestHandler CreateSut()
    {
        StaffUserBusinessRules rules = new(_users.Object, _userClaims.Object, _claims.Object);
        AuthBusinessRules authRules = new(_users.Object, _customers.Object);

        return new UpdateStaffUserRequestHandler(
            _users.Object,
            _claims.Object,
            _userClaims.Object,
            _branches.Object,
            rules,
            authRules,
            _currentUser.Object,
            _branchAuth.Object,
            _audit.Object,
            _staffSearch.Object);
    }

    private static ApplicationUser StaffUser(Guid id, string email, Guid? branchId = null) =>
        new()
        {
            Id = id,
            Email = email,
            IsActive = true,
            CustomerId = null,
            BranchId = branchId
        };

    private static UpdateStaffUserRequest BaseUpdate(Guid id, string email) =>
        new()
        {
            Id = id,
            Email = email,
            IsActive = true,
            IsManager = false
        };

    private void SetupActor(Guid userId, bool isBankManager, bool isSuperAdmin, Guid? branchId = null)
    {
        _currentUser.SetupGet(c => c.UserId).Returns(userId);
        _currentUser.SetupGet(c => c.IsBankManager).Returns(isBankManager);
        _currentUser.SetupGet(c => c.IsSuperAdmin).Returns(isSuperAdmin);
        _currentUser.SetupGet(c => c.BranchId).Returns(branchId);
    }

    private void SetupGetUser(ApplicationUser user)
    {
        _users
            .Setup(r => r.GetAsync(
                It.IsAny<Expression<Func<ApplicationUser, bool>>>(),
                It.IsAny<Func<IQueryable<ApplicationUser>, IIncludableQueryable<ApplicationUser, object>>>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
    }

    private void SetupBranch(Guid branchId)
    {
        _branches
            .Setup(r => r.GetAsync(
                It.IsAny<Expression<Func<Branch, bool>>>(),
                It.IsAny<Func<IQueryable<Branch>, IIncludableQueryable<Branch, object>>>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Branch { Id = branchId, Code = "NYC-01", Name = "NYC", IsActive = true });
    }

    private void SetupUpdateAndClaims(ApplicationUser user, bool isManager)
    {
        _users
            .Setup(r => r.UpdateAsync(It.IsAny<ApplicationUser>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationUser u, CancellationToken _) => u);

        List<OperationClaim> claims =
        [
            new() { Name = GeneralOperationClaims.BankStaff }
        ];
        if (isManager)
            claims.Add(new OperationClaim { Name = GeneralOperationClaims.BankManager });

        _users
            .Setup(r => r.GetClaimsAsync(It.IsAny<ApplicationUser>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(claims);

        // Email uniqueness: no other user with same email
        _users
            .Setup(r => r.AnyAsync(
                It.IsAny<Expression<Func<ApplicationUser, bool>>>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
    }

    private void SetupUserClaimForSync(Guid userId, Guid claimId, bool exists)
    {
        _userClaims
            .Setup(r => r.GetAsync(
                It.IsAny<Expression<Func<UserOperationClaim, bool>>>(),
                It.IsAny<Func<IQueryable<UserOperationClaim>, IIncludableQueryable<UserOperationClaim, object>>>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(exists
                ? new UserOperationClaim { UserId = userId, OperationClaimId = claimId }
                : null);
    }

    private void SetupClaimLookup(string name, Guid claimId)
    {
        _claims
            .Setup(r => r.GetAsync(
                It.Is<Expression<Func<OperationClaim, bool>>>(expr =>
                    expr.Compile()(new OperationClaim { Name = name })),
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
                    expr.Compile()(new UserOperationClaim { UserId = userId, OperationClaimId = claimId })),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(hasClaim);
    }
}
