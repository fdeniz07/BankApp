using System.Linq.Expressions;
using Application.Modules.Credit.Features.CreditApplications.Commands.Approve;
using Application.Modules.Credit.Features.CreditApplications.Constants;
using Application.Modules.Credit.Features.CreditApplications.Profiles;
using Application.Modules.Credit.Features.CreditApplications.Rules;
using Application.Services.Auth;
using Application.Services.Logging;
using Application.Services.Repositories;
using AutoMapper;
using Core.CrossCuttingConcerns.Exceptions.Types;
using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Application.UnitTests.Features.CreditApplications.Commands.Approve;

public class ApproveCreditApplicationRequestHandlerTests
{
    private readonly Mock<ICreditApplicationRepository> _creditApplicationRepository = new();
    private readonly Mock<IApplicationUserRepository> _applicationUserRepository = new();
    private readonly Mock<ICustomerRepository> _customerRepository = new();
    private readonly Mock<IIndividualCustomerRepository> _individualCustomerRepository = new();
    private readonly Mock<ICorporateCustomerRepository> _corporateCustomerRepository = new();
    private readonly Mock<ICreditSubTypeRepository> _creditSubTypeRepository = new();
    private readonly Mock<ICreditTypeRepository> _creditTypeRepository = new();
    private readonly Mock<ICurrentUserService> _currentUserService = new();
    private readonly Mock<IAuditLogger> _auditLogger = new();
    private readonly IMapper _mapper;
    private readonly ApproveCreditApplicationRequestHandler _sut;

    public ApproveCreditApplicationRequestHandlerTests()
    {
        MapperConfiguration configuration = new(
            cfg => cfg.AddProfile<MappingProfiles>(),
            NullLoggerFactory.Instance);
        _mapper = configuration.CreateMapper();

        CreditApplicationBusinessRules businessRules = new(
            _creditApplicationRepository.Object,
            _customerRepository.Object,
            _individualCustomerRepository.Object,
            _corporateCustomerRepository.Object,
            _creditSubTypeRepository.Object,
            _creditTypeRepository.Object);

        _sut = new ApproveCreditApplicationRequestHandler(
            _creditApplicationRepository.Object,
            _applicationUserRepository.Object,
            _mapper,
            businessRules,
            _currentUserService.Object,
            _auditLogger.Object);
    }

    [Fact]
    public async Task Handle_WhenApplicationMissing_ThrowsNotExists()
    {
        SetupGetApplication(null);

        ApproveCreditApplicationRequest request = new()
        {
            Id = Guid.NewGuid(),
            ApprovedAmount = 20_000,
            ApprovedTermMonths = 12,
            ApprovedInterestRate = 2.5m
        };

        Func<Task> act = () => _sut.Handle(request, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage(CreditApplicationMessages.NotExists);
    }

    [Fact]
    public async Task Handle_WhenNotPending_Throws()
    {
        Guid applicationId = Guid.NewGuid();
        Guid subTypeId = Guid.NewGuid();

        SetupGetApplication(new CreditApplication
        {
            Id = applicationId,
            ApplicationNumber = "APP-001",
            CreditSubTypeId = subTypeId,
            Status = CreditApplicationStatus.Rejected
        });

        ApproveCreditApplicationRequest request = new()
        {
            Id = applicationId,
            ApprovedAmount = 20_000,
            ApprovedTermMonths = 12,
            ApprovedInterestRate = 2.5m
        };

        Func<Task> act = () => _sut.Handle(request, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage(CreditApplicationMessages.OnlyPendingCanBeDecided);
    }

    [Fact]
    public async Task Handle_WhenPendingAndWithinLimits_ApprovesApplication()
    {
        Guid applicationId = Guid.NewGuid();
        Guid subTypeId = Guid.NewGuid();

        CreditApplication application = new()
        {
            Id = applicationId,
            ApplicationNumber = "APP-100",
            CreditSubTypeId = subTypeId,
            Status = CreditApplicationStatus.Pending,
            Notes = "initial"
        };

        SetupGetApplication(application);
        SetupCreditSubType(subTypeId, minAmount: 10_000, maxAmount: 100_000, minTerm: 6, maxTerm: 48);

        _creditApplicationRepository
            .Setup(r => r.UpdateAsync(It.IsAny<CreditApplication>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CreditApplication entity, CancellationToken _) => entity);

        ApproveCreditApplicationRequest request = new()
        {
            Id = applicationId,
            ApprovedAmount = 25_000,
            ApprovedTermMonths = 24,
            ApprovedInterestRate = 1.9m,
            DecisionBy = "staff@bank.local",
            Notes = "ok"
        };

        ApprovedCreditApplicationResponse response = await _sut.Handle(request, CancellationToken.None);

        response.Status.Should().Be(CreditApplicationStatus.Approved);
        response.ApprovedAmount.Should().Be(25_000);
        response.ApprovedTermMonths.Should().Be(24);
        response.ApprovedInterestRate.Should().Be(1.9m);
        response.DecisionBy.Should().Be("staff@bank.local");
        response.Notes.Should().Be("ok");
        response.DecisionDate.Should().NotBeNull();

        application.Status.Should().Be(CreditApplicationStatus.Approved);
        application.RejectionReason.Should().BeNull();

        _creditApplicationRepository.Verify(
            r => r.UpdateAsync(It.Is<CreditApplication>(c => c.Id == applicationId), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private void SetupGetApplication(CreditApplication? application)
    {
        _creditApplicationRepository
            .Setup(r => r.GetAsync(
                It.IsAny<Expression<Func<CreditApplication, bool>>>(),
                It.IsAny<Func<IQueryable<CreditApplication>, IIncludableQueryable<CreditApplication, object>>>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(application);
    }

    private void SetupCreditSubType(Guid subTypeId, decimal minAmount, decimal maxAmount, int minTerm, int maxTerm)
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
