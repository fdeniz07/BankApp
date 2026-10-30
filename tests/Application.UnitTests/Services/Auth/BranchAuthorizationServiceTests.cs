using System.Linq.Expressions;
using Application.Services.Auth;
using Application.Services.Logging;
using Application.Services.Repositories;
using Core.CrossCuttingConcerns.Exceptions.Types;
using Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Moq;

namespace Application.UnitTests.Services.Auth;

public class BranchAuthorizationServiceTests
{
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly Mock<ICustomerRepository> _customers = new();
    private readonly Mock<IAuditLogger> _audit = new();
    private readonly BranchAuthorizationService _sut;

    public BranchAuthorizationServiceTests()
    {
        _sut = new BranchAuthorizationService(_currentUser.Object, _customers.Object, _audit.Object);
    }

    [Fact]
    public async Task Staff_CannotViewOtherBranchCustomer()
    {
        Guid ownBranch = Guid.NewGuid();
        Guid otherBranch = Guid.NewGuid();
        Guid customerId = Guid.NewGuid();

        _currentUser.SetupGet(c => c.IsCustomer).Returns(false);
        _currentUser.SetupGet(c => c.IsBankStaff).Returns(true);
        _currentUser.SetupGet(c => c.IsBankManager).Returns(false);
        _currentUser.SetupGet(c => c.IsSuperAdmin).Returns(false);
        _currentUser.SetupGet(c => c.BranchId).Returns(ownBranch);
        _currentUser.SetupGet(c => c.UserId).Returns(Guid.NewGuid());

        SetupCustomer(new CustomerStub { Id = customerId, BranchId = otherBranch });

        Func<Task> act = () => _sut.EnsureCanViewCustomerAsync(customerId);

        await act.Should().ThrowAsync<AuthorizationException>();
    }

    [Fact]
    public async Task Manager_CannotModifyOtherBranchApplication()
    {
        Guid ownBranch = Guid.NewGuid();
        Guid otherBranch = Guid.NewGuid();

        _currentUser.SetupGet(c => c.IsCustomer).Returns(false);
        _currentUser.SetupGet(c => c.IsBankStaff).Returns(true);
        _currentUser.SetupGet(c => c.IsBankManager).Returns(true);
        _currentUser.SetupGet(c => c.IsSuperAdmin).Returns(false);
        _currentUser.SetupGet(c => c.BranchId).Returns(ownBranch);
        _currentUser.SetupGet(c => c.UserId).Returns(Guid.NewGuid());

        CreditApplication application = new()
        {
            Id = Guid.NewGuid(),
            BranchId = otherBranch,
            CustomerId = Guid.NewGuid()
        };

        Func<Task> act = () => _sut.EnsureCanModifyApplicationAsync(application);

        await act.Should().ThrowAsync<AuthorizationException>();
    }

    [Fact]
    public async Task SuperAdmin_CanModifyOtherBranchApplication()
    {
        _currentUser.SetupGet(c => c.IsCustomer).Returns(false);
        _currentUser.SetupGet(c => c.IsBankStaff).Returns(true);
        _currentUser.SetupGet(c => c.IsBankManager).Returns(true);
        _currentUser.SetupGet(c => c.IsSuperAdmin).Returns(true);
        _currentUser.SetupGet(c => c.BranchId).Returns((Guid?)null);
        _currentUser.SetupGet(c => c.UserId).Returns(Guid.NewGuid());

        CreditApplication application = new()
        {
            Id = Guid.NewGuid(),
            BranchId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid()
        };

        await _sut.EnsureCanModifyApplicationAsync(application);
    }

    [Fact]
    public void ResolveListBranchFilter_ManagerDefaultsToOwnBranch()
    {
        Guid ownBranch = Guid.NewGuid();
        _currentUser.SetupGet(c => c.IsCustomer).Returns(false);
        _currentUser.SetupGet(c => c.IsBankStaff).Returns(true);
        _currentUser.SetupGet(c => c.IsBankManager).Returns(true);
        _currentUser.SetupGet(c => c.IsSuperAdmin).Returns(false);
        _currentUser.SetupGet(c => c.BranchId).Returns(ownBranch);

        _sut.ResolveListBranchFilter(null).Should().Be(ownBranch);
        Guid other = Guid.NewGuid();
        _sut.ResolveListBranchFilter(other).Should().Be(other);
    }

    private void SetupCustomer(Customer customer)
    {
        _customers
            .Setup(r => r.GetAsync(
                It.IsAny<Expression<Func<Customer, bool>>>(),
                It.IsAny<Func<IQueryable<Customer>, IIncludableQueryable<Customer, object>>>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);
    }

    private sealed class CustomerStub : Customer;
}
