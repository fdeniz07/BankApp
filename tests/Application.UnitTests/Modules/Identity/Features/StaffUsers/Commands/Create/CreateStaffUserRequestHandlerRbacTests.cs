using Application.Modules.Identity.Features.Auth.Rules;
using Application.Modules.Identity.Features.StaffUsers.Commands.Create;
using Application.Modules.Identity.Features.StaffUsers.Constants;
using Application.Modules.Identity.Features.StaffUsers.Rules;
using Application.Services.Auth;
using Application.Services.Logging;
using Application.Services.Repositories;
using Application.Services.Search;
using Core.CrossCuttingConcerns.Exceptions.Types;
using FluentAssertions;
using Moq;

namespace Application.UnitTests.Modules.Identity.Features.StaffUsers.Commands.Create;

public class CreateStaffUserRequestHandlerRbacTests
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

    [Fact]
    public async Task Handle_WhenManagerCreatesWithIsManager_Throws()
    {
        _currentUser.SetupGet(c => c.IsSuperAdmin).Returns(false);
        _currentUser.SetupGet(c => c.IsBankManager).Returns(true);
        _currentUser.SetupGet(c => c.UserId).Returns(Guid.NewGuid());
        _currentUser.SetupGet(c => c.BranchId).Returns(Guid.NewGuid());

        CreateStaffUserRequest request = new()
        {
            Email = "new.manager@bank.local",
            Password = "Demo123!",
            IsManager = true
        };

        Func<Task> act = () => CreateSut().Handle(request, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage(StaffUserMessages.OnlySuperAdminMayCreateManager);
    }

    private CreateStaffUserRequestHandler CreateSut()
    {
        StaffUserBusinessRules staffRules = new(_users.Object, _userClaims.Object, _claims.Object);
        AuthBusinessRules authRules = new(_users.Object, _customers.Object);

        return new CreateStaffUserRequestHandler(
            _users.Object,
            _claims.Object,
            _userClaims.Object,
            _branches.Object,
            authRules,
            staffRules,
            _currentUser.Object,
            _branchAuth.Object,
            _audit.Object,
            _staffSearch.Object);
    }
}
