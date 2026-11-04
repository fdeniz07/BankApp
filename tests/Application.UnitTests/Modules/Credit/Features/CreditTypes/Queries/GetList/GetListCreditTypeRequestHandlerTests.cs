using System.Linq.Expressions;
using Application.Modules.Credit.Features.CreditTypes.Queries.GetList;
using Application.Services.Branches;
using Application.Services.Repositories;
using Application.Services.Search;
using AutoMapper;
using Core.Application.Requests;
using Core.Application.Responses;
using Core.Repositories;
using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Application.UnitTests.Modules.Credit.Features.CreditTypes.Queries.GetList;

public class GetListCreditTypeRequestHandlerTests
{
    private readonly Mock<ICreditTypeRepository> _repo = new();
    private readonly Mock<ICatalogLocaleService> _locale = new();
    private readonly Mock<ICreditTypeSearchService> _search = new();
    private readonly IMapper _mapper;

    public GetListCreditTypeRequestHandlerTests()
    {
        MapperConfiguration cfg = new(
            c => c.AddProfile(new Application.Modules.Credit.Features.CreditTypes.Profiles.MappingProfiles()),
            NullLoggerFactory.Instance);
        _mapper = cfg.CreateMapper();
        _locale
            .Setup(l => l.ResolveForcedCatalogLocaleAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);
    }

    [Fact]
    public async Task Handle_WhenSearchAvailable_UsesElasticsearch()
    {
        _search.SetupGet(s => s.IsAvailable).Returns(true);
        _search
            .Setup(s => s.SearchAsync(It.IsAny<CreditTypeSearchQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetListResponse<GetListCreditTypeListItemDto>
            {
                Items =
                [
                    new GetListCreditTypeListItemDto
                    {
                        Id = Guid.NewGuid(),
                        Name = "Home",
                        Code = "HOME",
                        Locale = "en",
                        Category = CreditCategory.Individual,
                        CurrencyCode = "USD",
                        RiskLevel = CreditRiskLevel.Low,
                        IsActive = true
                    }
                ],
                Index = 0,
                Size = 50,
                Count = 1,
                Pages = 1
            });

        GetListResponse<GetListCreditTypeListItemDto> response = await CreateSut().Handle(
            new GetListCreditTypeRequest
            {
                PageRequest = new PageRequest { PageIndex = 0, PageSize = 50 },
                Search = "Home",
                CurrencyCode = "USD"
            },
            CancellationToken.None);

        response.Items.Should().ContainSingle(i => i.Code == "HOME");
        _repo.Verify(
            r => r.GetListAsync(
                It.IsAny<Expression<Func<CreditType, bool>>>(),
                It.IsAny<Func<IQueryable<CreditType>, IOrderedQueryable<CreditType>>>(),
                It.IsAny<Func<IQueryable<CreditType>, IIncludableQueryable<CreditType, object>>>(),
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
        _search.SetupGet(s => s.IsAvailable).Returns(true);
        _search
            .Setup(s => s.SearchAsync(It.IsAny<CreditTypeSearchQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetListResponse<GetListCreditTypeListItemDto>?)null);

        CreditType type = new()
        {
            Id = Guid.NewGuid(),
            Name = "Car",
            Code = "CAR",
            Locale = "tr",
            Category = CreditCategory.Individual,
            CurrencyCode = "TRY",
            RiskLevel = CreditRiskLevel.Medium,
            IsActive = true
        };
        SetupSql([type]);

        GetListResponse<GetListCreditTypeListItemDto> response = await CreateSut().Handle(
            new GetListCreditTypeRequest
            {
                PageRequest = new PageRequest { PageIndex = 0, PageSize = 50 },
                Search = "Car"
            },
            CancellationToken.None);

        response.Items.Should().ContainSingle(i => i.Code == "CAR");
    }

    private GetListCreditTypeRequestHandler CreateSut() =>
        new(_repo.Object, _mapper, _locale.Object, _search.Object);

    private void SetupSql(List<CreditType> items)
    {
        Mock<IPaginate<CreditType>> page = new();
        page.SetupGet(p => p.Items).Returns(items);
        page.SetupGet(p => p.From).Returns(0);
        page.SetupGet(p => p.Index).Returns(0);
        page.SetupGet(p => p.Size).Returns(50);
        page.SetupGet(p => p.Count).Returns(items.Count);
        page.SetupGet(p => p.Pages).Returns(1);

        _repo
            .Setup(r => r.GetListAsync(
                It.IsAny<Expression<Func<CreditType, bool>>>(),
                It.IsAny<Func<IQueryable<CreditType>, IOrderedQueryable<CreditType>>>(),
                It.IsAny<Func<IQueryable<CreditType>, IIncludableQueryable<CreditType, object>>>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(page.Object);
    }
}
