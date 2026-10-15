using System.Linq.Expressions;
using Application.Features.CreditApplications.Constants;
using Application.Features.CreditApplications.Rules;
using Application.Services.Repositories;
using Core.CrossCuttingConcerns.Exceptions.Types;
using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Moq;

namespace Application.UnitTests.Features.CreditApplications.Rules;

public class CreditApplicationBusinessRulesTests
{
    private readonly Mock<ICreditApplicationRepository> _creditApplicationRepository = new();
    private readonly Mock<ICustomerRepository> _customerRepository = new();
    private readonly Mock<IIndividualCustomerRepository> _individualCustomerRepository = new();
    private readonly Mock<ICorporateCustomerRepository> _corporateCustomerRepository = new();
    private readonly Mock<ICreditSubTypeRepository> _creditSubTypeRepository = new();
    private readonly Mock<ICreditTypeRepository> _creditTypeRepository = new();
    private readonly CreditApplicationBusinessRules _sut;

    public CreditApplicationBusinessRulesTests()
    {
        _sut = new CreditApplicationBusinessRules(
            _creditApplicationRepository.Object,
            _customerRepository.Object,
            _individualCustomerRepository.Object,
            _corporateCustomerRepository.Object,
            _creditSubTypeRepository.Object,
            _creditTypeRepository.Object);
    }

    [Fact]
    public async Task CreditApplicationShouldExistWhenSelected_WhenNull_Throws()
    {
        Func<Task> act = () => _sut.CreditApplicationShouldExistWhenSelected(null);

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage(CreditApplicationMessages.NotExists);
    }

    [Fact]
    public async Task ApplicationShouldBePending_WhenApproved_Throws()
    {
        CreditApplication application = new()
        {
            Status = CreditApplicationStatus.Approved
        };

        Func<Task> act = () => _sut.ApplicationShouldBePending(application);

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage(CreditApplicationMessages.OnlyPendingCanBeDecided);
    }

    [Fact]
    public async Task ApplicationShouldBePending_WhenPending_DoesNotThrow()
    {
        CreditApplication application = new()
        {
            Status = CreditApplicationStatus.Pending
        };

        Func<Task> act = () => _sut.ApplicationShouldBePending(application);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task RequestShouldBeWithinCreditSubTypeLimits_WhenAmountOutOfRange_Throws()
    {
        Guid subTypeId = Guid.NewGuid();
        SetupCreditSubType(subTypeId, minAmount: 10_000, maxAmount: 50_000, minTerm: 6, maxTerm: 36);

        Func<Task> act = () => _sut.RequestShouldBeWithinCreditSubTypeLimits(subTypeId, 5_000, 12);

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage(CreditApplicationMessages.AmountOutOfRange);
    }

    [Fact]
    public async Task RequestShouldBeWithinCreditSubTypeLimits_WhenTermOutOfRange_Throws()
    {
        Guid subTypeId = Guid.NewGuid();
        SetupCreditSubType(subTypeId, minAmount: 10_000, maxAmount: 50_000, minTerm: 6, maxTerm: 36);

        Func<Task> act = () => _sut.RequestShouldBeWithinCreditSubTypeLimits(subTypeId, 20_000, 48);

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage(CreditApplicationMessages.TermOutOfRange);
    }

    [Fact]
    public async Task RequestShouldBeWithinCreditSubTypeLimits_WhenWithinLimits_DoesNotThrow()
    {
        Guid subTypeId = Guid.NewGuid();
        SetupCreditSubType(subTypeId, minAmount: 10_000, maxAmount: 50_000, minTerm: 6, maxTerm: 36);

        Func<Task> act = () => _sut.RequestShouldBeWithinCreditSubTypeLimits(subTypeId, 20_000, 12);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task CustomerCategoryShouldMatchCreditSubType_WhenIndividualCustomerAndCorporateCredit_Throws()
    {
        Guid customerId = Guid.NewGuid();
        Guid creditTypeId = Guid.NewGuid();
        Guid subTypeId = Guid.NewGuid();

        _creditSubTypeRepository
            .Setup(r => r.GetAsync(
                It.IsAny<Expression<Func<CreditSubType, bool>>>(),
                It.IsAny<Func<IQueryable<CreditSubType>, IIncludableQueryable<CreditSubType, object>>>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CreditSubType
            {
                Id = subTypeId,
                CreditTypeId = creditTypeId,
                IsActive = true
            });

        _creditTypeRepository
            .Setup(r => r.GetAsync(
                It.IsAny<Expression<Func<CreditType, bool>>>(),
                It.IsAny<Func<IQueryable<CreditType>, IIncludableQueryable<CreditType, object>>>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CreditType
            {
                Id = creditTypeId,
                Category = CreditCategory.Corporate
            });

        _individualCustomerRepository
            .Setup(r => r.AnyAsync(It.IsAny<Expression<Func<IndividualCustomer, bool>>>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _corporateCustomerRepository
            .Setup(r => r.AnyAsync(It.IsAny<Expression<Func<CorporateCustomer, bool>>>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        Func<Task> act = () => _sut.CustomerCategoryShouldMatchCreditSubType(customerId, subTypeId);

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage(CreditApplicationMessages.CategoryMismatch);
    }

    private void SetupCreditSubType(
        Guid subTypeId,
        decimal minAmount,
        decimal maxAmount,
        int minTerm,
        int maxTerm)
    {
        _creditSubTypeRepository
            .Setup(r => r.GetAsync(
                It.IsAny<Expression<Func<CreditSubType, bool>>>(),
                It.IsAny<Func<IQueryable<CreditSubType>, IIncludableQueryable<CreditSubType, object>>>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CreditSubType
            {
                Id = subTypeId,
                MinAmount = minAmount,
                MaxAmount = maxAmount,
                MinTermMonths = minTerm,
                MaxTermMonths = maxTerm,
                IsActive = true
            });
    }
}
