using System.Linq.Expressions;
using Application.Modules.Customers.Features.IndividualCustomers.Queries.GetList;
using Application.Services.Auth;
using Application.Services.Repositories;
using Application.Services.Search;
using AutoMapper;
using Core.Application.Requests;
using Core.Application.Responses;
using Core.Repositories;
using Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Application.UnitTests.Modules.Customers.Features.IndividualCustomers.Queries.GetList;

public class GetListIndividualCustomerRequestHandlerTests
{
    private readonly Mock<IIndividualCustomerRepository> _repo = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly Mock<IBranchAuthorizationService> _branchAuth = new();
    private readonly Mock<ICustomerSearchService> _search = new();
    private readonly IMapper _mapper;

    public GetListIndividualCustomerRequestHandlerTests()
    {
        MapperConfiguration cfg = new(
            c => c.AddProfile(new Application.Modules.Customers.Features.IndividualCustomers.Profiles.MappingProfiles()),
            NullLoggerFactory.Instance);
        _mapper = cfg.CreateMapper();
    }

    [Fact]
    public async Task Handle_WhenSearchAvailable_UsesElasticsearch()
    {
        _branchAuth.SetupGet(b => b.MustSeeNoBranchData).Returns(false);
        _branchAuth.Setup(b => b.ResolveListBranchFilter(It.IsAny<Guid?>())).Returns((Guid?)null);
        _currentUser.SetupGet(c => c.IsCustomer).Returns(false);
        _currentUser.SetupGet(c => c.IsSuperAdmin).Returns(true);
        _currentUser.SetupGet(c => c.IsBankManager).Returns(true);
        _search.SetupGet(s => s.IsAvailable).Returns(true);
        _search
            .Setup(s => s.SearchAsync(It.IsAny<CustomerSearchQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CustomerSearchPage
            {
                Items =
                [
                    new CustomerSearchDocument
                    {
                        Id = Guid.NewGuid(),
                        Kind = CustomerSearchKinds.Individual,
                        CustomerNumber = "IND-1",
                        Email = "a@bank.local",
                        PhoneNumber = "1",
                        FirstName = "Ada",
                        LastName = "Lovelace",
                        NationalId = "1",
                        IsActive = true
                    }
                ],
                Index = 0,
                Size = 10,
                Count = 1,
                Pages = 1
            });

        GetListResponse<GetListIndividualCustomerListItemDto> response = await CreateSut().Handle(
            new GetListIndividualCustomerRequest
            {
                PageRequest = new PageRequest { PageIndex = 0, PageSize = 10 },
                Search = "Ada",
                IsActive = true
            },
            CancellationToken.None);

        response.Items.Should().ContainSingle(i => i.FirstName == "Ada");
        _repo.Verify(
            r => r.GetListAsync(
                It.IsAny<Expression<Func<IndividualCustomer, bool>>>(),
                It.IsAny<Func<IQueryable<IndividualCustomer>, IOrderedQueryable<IndividualCustomer>>>(),
                It.IsAny<Func<IQueryable<IndividualCustomer>, IIncludableQueryable<IndividualCustomer, object>>>(),
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
        _branchAuth.SetupGet(b => b.MustSeeNoBranchData).Returns(false);
        _branchAuth.Setup(b => b.ResolveListBranchFilter(It.IsAny<Guid?>())).Returns((Guid?)null);
        _currentUser.SetupGet(c => c.IsCustomer).Returns(false);
        _currentUser.SetupGet(c => c.IsSuperAdmin).Returns(true);
        _currentUser.SetupGet(c => c.IsBankManager).Returns(true);
        _search.SetupGet(s => s.IsAvailable).Returns(true);
        _search
            .Setup(s => s.SearchAsync(It.IsAny<CustomerSearchQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CustomerSearchPage?)null);

        IndividualCustomer customer = new()
        {
            Id = Guid.NewGuid(),
            CustomerNumber = "IND-2",
            Email = "b@bank.local",
            PhoneNumber = "2",
            Address = "x",
            FirstName = "Grace",
            LastName = "Hopper",
            NationalId = "2",
            IsActive = true
        };
        SetupSql([customer]);

        GetListResponse<GetListIndividualCustomerListItemDto> response = await CreateSut().Handle(
            new GetListIndividualCustomerRequest
            {
                PageRequest = new PageRequest { PageIndex = 0, PageSize = 10 },
                Search = "Grace",
                IsActive = true
            },
            CancellationToken.None);

        response.Items.Should().ContainSingle(i => i.FirstName == "Grace");
    }

    private GetListIndividualCustomerRequestHandler CreateSut() =>
        new(_repo.Object, _mapper, _currentUser.Object, _branchAuth.Object, _search.Object);

    private void SetupSql(List<IndividualCustomer> items)
    {
        Mock<IPaginate<IndividualCustomer>> page = new();
        page.SetupGet(p => p.Items).Returns(items);
        page.SetupGet(p => p.From).Returns(0);
        page.SetupGet(p => p.Index).Returns(0);
        page.SetupGet(p => p.Size).Returns(10);
        page.SetupGet(p => p.Count).Returns(items.Count);
        page.SetupGet(p => p.Pages).Returns(1);

        _repo
            .Setup(r => r.GetListAsync(
                It.IsAny<Expression<Func<IndividualCustomer, bool>>>(),
                It.IsAny<Func<IQueryable<IndividualCustomer>, IOrderedQueryable<IndividualCustomer>>>(),
                It.IsAny<Func<IQueryable<IndividualCustomer>, IIncludableQueryable<IndividualCustomer, object>>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(page.Object);
    }
}
