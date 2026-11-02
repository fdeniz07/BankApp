using System.Linq.Expressions;
using Application.Modules.Credit.Features.CreditApplications.Queries.GetFacets;
using Application.Services.Auth;
using Application.Services.Repositories;
using Application.Services.Search;
using Core.Repositories;
using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Moq;

namespace Application.UnitTests.Features.CreditApplications.Queries.GetFacets;

public class GetCreditApplicationFacetsRequestHandlerTests
{
    private readonly Mock<ICreditApplicationSearchService> _search = new();
    private readonly Mock<ICreditApplicationRepository> _applications = new();
    private readonly Mock<ICreditSubTypeRepository> _subTypes = new();
    private readonly Mock<ICreditTypeRepository> _creditTypes = new();
    private readonly Mock<ICreditApplicationDocumentRepository> _documents = new();
    private readonly Mock<IBranchRepository> _branches = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly Mock<IBranchAuthorizationService> _branchAuthorization = new();

    private GetCreditApplicationFacetsRequestHandler CreateSut() =>
        new(
            _search.Object,
            _applications.Object,
            _subTypes.Object,
            _creditTypes.Object,
            _documents.Object,
            _branches.Object,
            _currentUser.Object,
            _branchAuthorization.Object);

    [Fact]
    public async Task Handle_WhenMustSeeNoBranchData_ReturnsEmptySql()
    {
        _branchAuthorization.SetupGet(s => s.MustSeeNoBranchData).Returns(true);

        CreditApplicationFacetsResponse response =
            await CreateSut().Handle(new GetCreditApplicationFacetsRequest(), CancellationToken.None);

        response.Source.Should().Be("Sql");
        response.Statuses.Should().BeEmpty();
        response.AmountBuckets.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenElasticsearchReturnsFacets_UsesEsSourceAndLabelsBranches()
    {
        Guid branchId = Guid.NewGuid();
        _branchAuthorization.SetupGet(s => s.MustSeeNoBranchData).Returns(false);
        _branchAuthorization.Setup(s => s.ResolveListBranchFilter(null)).Returns((Guid?)null);
        _currentUser.SetupGet(u => u.IsSuperAdmin).Returns(true);
        _currentUser.SetupGet(u => u.IsBankManager).Returns(true);
        _search.SetupGet(s => s.IsAvailable).Returns(true);
        _search
            .Setup(s => s.GetFacetsAsync(It.IsAny<CreditApplicationSearchQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CreditApplicationFacetsResponse
            {
                Source = "Elasticsearch",
                Statuses = [new FacetBucketDto { Key = "Pending", Count = 2 }],
                Branches = [new FacetBucketDto { Key = branchId.ToString(), Count = 2 }],
                AmountBuckets =
                [
                    new AmountBucketDto { From = 0, To = 50_000, Count = 1 }
                ]
            });

        _branches
            .Setup(r => r.GetListAsync(
                It.IsAny<Expression<Func<Branch, bool>>>(),
                It.IsAny<Func<IQueryable<Branch>, IOrderedQueryable<Branch>>>(),
                It.IsAny<Func<IQueryable<Branch>, IIncludableQueryable<Branch, object>>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Paginate<Branch>
            {
                Items =
                [
                    new Branch
                    {
                        Id = branchId,
                        Code = "NYC",
                        Name = "New York",
                        IsActive = true
                    }
                ],
                Count = 1,
                Size = 1,
                Pages = 1
            });

        CreditApplicationFacetsResponse response =
            await CreateSut().Handle(new GetCreditApplicationFacetsRequest(), CancellationToken.None);

        response.Source.Should().Be("Elasticsearch");
        response.Statuses.Should().ContainSingle(s => s.Key == "Pending" && s.Count == 2);
        response.Branches.Should().ContainSingle(b => b.Label == "NYC");
    }

    [Fact]
    public async Task Handle_WhenEsUnavailable_FallsBackToSqlGrouping()
    {
        Guid branchId = Guid.NewGuid();
        Guid subTypeId = Guid.NewGuid();
        Guid typeId = Guid.NewGuid();

        _branchAuthorization.SetupGet(s => s.MustSeeNoBranchData).Returns(false);
        _branchAuthorization.Setup(s => s.ResolveListBranchFilter(null)).Returns((Guid?)null);
        _currentUser.SetupGet(u => u.IsSuperAdmin).Returns(true);
        _currentUser.SetupGet(u => u.IsBankManager).Returns(true);
        _search.SetupGet(s => s.IsAvailable).Returns(false);

        _applications
            .Setup(r => r.GetListAsync(
                It.IsAny<Expression<Func<CreditApplication, bool>>>(),
                It.IsAny<Func<IQueryable<CreditApplication>, IOrderedQueryable<CreditApplication>>>(),
                It.IsAny<Func<IQueryable<CreditApplication>, IIncludableQueryable<CreditApplication, object>>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Paginate<CreditApplication>
            {
                Items =
                [
                    new CreditApplication
                    {
                        Id = Guid.NewGuid(),
                        ApplicationNumber = "APP-1",
                        Status = CreditApplicationStatus.Pending,
                        BranchId = branchId,
                        CreditSubTypeId = subTypeId,
                        RequestedAmount = 25_000m,
                        RequestedTermMonths = 12,
                        CustomerId = Guid.NewGuid(),
                        ApplicationDate = DateTimeOffset.UtcNow
                    },
                    new CreditApplication
                    {
                        Id = Guid.NewGuid(),
                        ApplicationNumber = "APP-2",
                        Status = CreditApplicationStatus.Approved,
                        BranchId = branchId,
                        CreditSubTypeId = subTypeId,
                        RequestedAmount = 75_000m,
                        RequestedTermMonths = 24,
                        CustomerId = Guid.NewGuid(),
                        ApplicationDate = DateTimeOffset.UtcNow
                    }
                ],
                Count = 2,
                Size = 500,
                Pages = 1
            });

        _documents
            .Setup(r => r.GetListAsync(
                It.IsAny<Expression<Func<CreditApplicationDocument, bool>>>(),
                It.IsAny<Func<IQueryable<CreditApplicationDocument>, IOrderedQueryable<CreditApplicationDocument>>>(),
                It.IsAny<Func<IQueryable<CreditApplicationDocument>, IIncludableQueryable<CreditApplicationDocument, object>>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Paginate<CreditApplicationDocument>
            {
                Items = [],
                Count = 0,
                Size = 50,
                Pages = 0
            });

        _subTypes
            .Setup(r => r.GetListAsync(
                It.IsAny<Expression<Func<CreditSubType, bool>>>(),
                It.IsAny<Func<IQueryable<CreditSubType>, IOrderedQueryable<CreditSubType>>>(),
                It.IsAny<Func<IQueryable<CreditSubType>, IIncludableQueryable<CreditSubType, object>>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Paginate<CreditSubType>
            {
                Items =
                [
                    new CreditSubType
                    {
                        Id = subTypeId,
                        CreditTypeId = typeId,
                        Name = "Personal",
                        InterestRate = 2.5m
                    }
                ],
                Count = 1,
                Size = 1,
                Pages = 1
            });

        _creditTypes
            .Setup(r => r.GetListAsync(
                It.IsAny<Expression<Func<CreditType, bool>>>(),
                It.IsAny<Func<IQueryable<CreditType>, IOrderedQueryable<CreditType>>>(),
                It.IsAny<Func<IQueryable<CreditType>, IIncludableQueryable<CreditType, object>>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Paginate<CreditType>
            {
                Items =
                [
                    new CreditType { Id = typeId, Name = "Consumer", CurrencyCode = "USD" }
                ],
                Count = 1,
                Size = 1,
                Pages = 1
            });

        _branches
            .Setup(r => r.GetListAsync(
                It.IsAny<Expression<Func<Branch, bool>>>(),
                It.IsAny<Func<IQueryable<Branch>, IOrderedQueryable<Branch>>>(),
                It.IsAny<Func<IQueryable<Branch>, IIncludableQueryable<Branch, object>>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Paginate<Branch>
            {
                Items =
                [
                    new Branch { Id = branchId, Code = "CHI", Name = "Chicago", IsActive = true }
                ],
                Count = 1,
                Size = 1,
                Pages = 1
            });

        CreditApplicationFacetsResponse response =
            await CreateSut().Handle(new GetCreditApplicationFacetsRequest(), CancellationToken.None);

        response.Source.Should().Be("Sql");
        response.Statuses.Should().HaveCount(2);
        response.Currencies.Should().ContainSingle(c => c.Key == "USD" && c.Count == 2);
        response.Branches.Should().ContainSingle(b => b.Label == "CHI");
        response.AmountBuckets.Should().HaveCount(5);
        response.AmountBuckets[0].Count.Should().Be(1);
        response.AmountBuckets[1].Count.Should().Be(1);
        response.MissingRequiredDocuments.True.Should().Be(1);
    }
}
