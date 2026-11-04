using System.Linq.Expressions;
using Application.Modules.Identity.Features.StaffUsers.Queries.GetList;
using Application.Services.Auth;
using Application.Services.Repositories;
using Application.Services.Search;
using Core.Application.Requests;
using Core.Application.Responses;
using Core.Repositories;
using Core.Security.Constants;
using Core.Security.Entities;
using Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Moq;

namespace Application.UnitTests.Modules.Identity.Features.StaffUsers.Queries.GetList;

public class GetListStaffUserRequestHandlerTests
{
    private readonly Mock<IApplicationUserRepository> _users = new();
    private readonly Mock<IBranchAuthorizationService> _branchAuth = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly Mock<IStaffUserSearchService> _search = new();

    [Fact]
    public async Task Handle_WhenSearchAvailable_UsesElasticsearchResult()
    {
        Guid branchId = Guid.NewGuid();
        _branchAuth.Setup(b => b.MustSeeNoBranchData).Returns(false);
        _branchAuth.Setup(b => b.ResolveListBranchFilter(branchId)).Returns(branchId);
        _currentUser.SetupGet(c => c.IsSuperAdmin).Returns(true);
        _currentUser.SetupGet(c => c.IsBankManager).Returns(true);
        _search.SetupGet(s => s.IsAvailable).Returns(true);

        GetListResponse<GetListStaffUserListItemDto> esResult = new()
        {
            Items =
            [
                new GetListStaffUserListItemDto
                {
                    Id = Guid.NewGuid(),
                    Email = "staff.nyc@bank.local",
                    IsActive = true,
                    BranchId = branchId
                }
            ],
            Index = 0,
            Size = 10,
            Count = 1,
            Pages = 1
        };

        _search
            .Setup(s => s.SearchAsync(It.IsAny<StaffUserSearchQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(esResult);

        GetListResponse<GetListStaffUserListItemDto> response = await CreateSut().Handle(
            new GetListStaffUserRequest
            {
                PageRequest = new PageRequest { PageIndex = 0, PageSize = 10 },
                BranchId = branchId,
                Search = "nyc"
            },
            CancellationToken.None);

        response.Items.Should().HaveCount(1);
        response.Items[0].Email.Should().Be("staff.nyc@bank.local");
        _users.Verify(
            r => r.GetListAsync(
                It.IsAny<Expression<Func<ApplicationUser, bool>>>(),
                It.IsAny<Func<IQueryable<ApplicationUser>, IOrderedQueryable<ApplicationUser>>>(),
                It.IsAny<Func<IQueryable<ApplicationUser>, IIncludableQueryable<ApplicationUser, object>>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenElasticsearchReturnsNull_FallsBackToSql()
    {
        _branchAuth.Setup(b => b.MustSeeNoBranchData).Returns(false);
        _branchAuth.Setup(b => b.ResolveListBranchFilter(null)).Returns((Guid?)null);
        _currentUser.SetupGet(c => c.IsSuperAdmin).Returns(true);
        _currentUser.SetupGet(c => c.IsBankManager).Returns(true);
        _search.SetupGet(s => s.IsAvailable).Returns(true);
        _search
            .Setup(s => s.SearchAsync(It.IsAny<StaffUserSearchQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetListResponse<GetListStaffUserListItemDto>?)null);

        ApplicationUser staff = new()
        {
            Id = Guid.NewGuid(),
            Email = "staff@bank.local",
            IsActive = true,
            CustomerId = null,
            UserOperationClaims =
            [
                new UserOperationClaim
                {
                    OperationClaim = new OperationClaim { Name = GeneralOperationClaims.BankStaff }
                }
            ]
        };

        SetupSqlList([staff]);

        GetListResponse<GetListStaffUserListItemDto> response = await CreateSut().Handle(
            new GetListStaffUserRequest
            {
                PageRequest = new PageRequest { PageIndex = 0, PageSize = 10 },
                Search = "staff"
            },
            CancellationToken.None);

        response.Items.Should().ContainSingle(i => i.Email == "staff@bank.local");
        response.Items[0].IsManager.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenNoSearchFilters_UsesSqlDirectly()
    {
        _branchAuth.Setup(b => b.MustSeeNoBranchData).Returns(false);
        _branchAuth.Setup(b => b.ResolveListBranchFilter(null)).Returns((Guid?)null);
        _search.SetupGet(s => s.IsAvailable).Returns(true);

        ApplicationUser staff = new()
        {
            Id = Guid.NewGuid(),
            Email = "plain@bank.local",
            IsActive = true,
            CustomerId = null,
            UserOperationClaims = []
        };
        SetupSqlList([staff]);

        GetListResponse<GetListStaffUserListItemDto> response = await CreateSut().Handle(
            new GetListStaffUserRequest
            {
                PageRequest = new PageRequest { PageIndex = 0, PageSize = 10 }
            },
            CancellationToken.None);

        response.Items.Should().ContainSingle();
        _search.Verify(
            s => s.SearchAsync(It.IsAny<StaffUserSearchQuery>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private GetListStaffUserRequestHandler CreateSut() =>
        new(_users.Object, _branchAuth.Object, _currentUser.Object, _search.Object);

    private void SetupSqlList(List<ApplicationUser> items)
    {
        Mock<IPaginate<ApplicationUser>> page = new();
        page.SetupGet(p => p.Items).Returns(items);
        page.SetupGet(p => p.From).Returns(0);
        page.SetupGet(p => p.Index).Returns(0);
        page.SetupGet(p => p.Size).Returns(10);
        page.SetupGet(p => p.Count).Returns(items.Count);
        page.SetupGet(p => p.Pages).Returns(1);

        _users
            .Setup(r => r.GetListAsync(
                It.IsAny<Expression<Func<ApplicationUser, bool>>>(),
                It.IsAny<Func<IQueryable<ApplicationUser>, IOrderedQueryable<ApplicationUser>>>(),
                It.IsAny<Func<IQueryable<ApplicationUser>, IIncludableQueryable<ApplicationUser, object>>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(page.Object);
    }
}
