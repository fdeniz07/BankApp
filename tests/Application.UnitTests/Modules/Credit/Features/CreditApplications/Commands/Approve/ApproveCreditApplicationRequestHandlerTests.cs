using System.Linq.Expressions;
using Application.Modules.Credit.Features.CreditApplicationDocuments;
using Application.Modules.Credit.Features.CreditApplications.ApprovalAuthority;
using Application.Modules.Credit.Features.CreditApplications.Commands.Approve;
using Application.Modules.Credit.Features.CreditApplications.Constants;
using Application.Modules.Credit.Features.CreditApplications.Profiles;
using Application.Modules.Credit.Features.CreditApplications.Rules;
using Application.Services.Auth;
using Application.Services.Logging;
using Application.Services.Repositories;
using AutoMapper;
using Core.CrossCuttingConcerns.Exceptions.Types;
using Core.Repositories;
using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace Application.UnitTests.Features.CreditApplications.Commands.Approve;

public class ApproveCreditApplicationRequestHandlerTests
{
    private readonly Mock<ICreditApplicationRepository> _creditApplicationRepository = new();
    private readonly Mock<ICreditApplicationDocumentRepository> _documentRepository = new();
    private readonly Mock<IApplicationUserRepository> _applicationUserRepository = new();
    private readonly Mock<IBranchRepository> _branchRepository = new();
    private readonly Mock<ICustomerRepository> _customerRepository = new();
    private readonly Mock<IIndividualCustomerRepository> _individualCustomerRepository = new();
    private readonly Mock<ICorporateCustomerRepository> _corporateCustomerRepository = new();
    private readonly Mock<ICreditSubTypeRepository> _creditSubTypeRepository = new();
    private readonly Mock<ICreditTypeRepository> _creditTypeRepository = new();
    private readonly Mock<ICurrentUserService> _currentUserService = new();
    private readonly Mock<IBranchAuthorizationService> _branchAuthorization = new();
    private readonly Mock<IAuditLogger> _auditLogger = new();
    private readonly IMapper _mapper;
    private CreditDocumentsOptions _documentsOptions = new() { ApproveGateMode = ApproveDocumentGateMode.None };
    private ApprovalAuthorityOptions _authorityOptions = new() { Enabled = false };

    public ApproveCreditApplicationRequestHandlerTests()
    {
        MapperConfiguration configuration = new(
            cfg => cfg.AddProfile<MappingProfiles>(),
            NullLoggerFactory.Instance);
        _mapper = configuration.CreateMapper();

        _branchAuthorization
            .Setup(s => s.EnsureCanModifyApplicationAsync(It.IsAny<CreditApplication>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    private ApproveCreditApplicationRequestHandler CreateSut()
    {
        CreditApplicationBusinessRules businessRules = new(
            _creditApplicationRepository.Object,
            _customerRepository.Object,
            _individualCustomerRepository.Object,
            _corporateCustomerRepository.Object,
            _creditSubTypeRepository.Object,
            _creditTypeRepository.Object);

        return new ApproveCreditApplicationRequestHandler(
            _creditApplicationRepository.Object,
            _documentRepository.Object,
            _applicationUserRepository.Object,
            _branchRepository.Object,
            _mapper,
            businessRules,
            _currentUserService.Object,
            _branchAuthorization.Object,
            _auditLogger.Object,
            Options.Create(_documentsOptions),
            Options.Create(_authorityOptions));
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

        Func<Task> act = () => CreateSut().Handle(request, CancellationToken.None);

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

        Func<Task> act = () => CreateSut().Handle(request, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage(CreditApplicationMessages.OnlyPendingCanBeDecided);
    }

    [Fact]
    public async Task Handle_WhenHardGateAndMissingDocs_Throws()
    {
        _documentsOptions = new CreditDocumentsOptions { ApproveGateMode = ApproveDocumentGateMode.Hard };
        Guid applicationId = Guid.NewGuid();
        Guid subTypeId = Guid.NewGuid();

        SetupGetApplication(new CreditApplication
        {
            Id = applicationId,
            ApplicationNumber = "APP-HARD",
            CreditSubTypeId = subTypeId,
            Status = CreditApplicationStatus.Pending
        });
        SetupCreditSubType(subTypeId, minAmount: 10_000, maxAmount: 100_000, minTerm: 6, maxTerm: 48);
        SetupDocuments([]);

        ApproveCreditApplicationRequest request = new()
        {
            Id = applicationId,
            ApprovedAmount = 25_000,
            ApprovedTermMonths = 24,
            ApprovedInterestRate = 1.9m
        };

        Func<Task> act = () => CreateSut().Handle(request, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage(CreditApplicationMessages.MissingRequiredDocuments);
    }

    [Fact]
    public async Task Handle_WhenAmountExceedsAuthority_Throws()
    {
        _authorityOptions = new ApprovalAuthorityOptions
        {
            Enabled = true,
            LimitsByPosition = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
            {
                [StaffPosition.CreditAnalyst.Name] = 20_000m
            },
            UnlimitedRoles = ["BankManager", "SuperAdmin"]
        };

        Guid applicationId = Guid.NewGuid();
        Guid subTypeId = Guid.NewGuid();
        Guid staffUserId = Guid.NewGuid();

        SetupGetApplication(new CreditApplication
        {
            Id = applicationId,
            ApplicationNumber = "APP-AUTH",
            CreditSubTypeId = subTypeId,
            Status = CreditApplicationStatus.Pending
        });
        SetupCreditSubType(subTypeId, minAmount: 10_000, maxAmount: 100_000, minTerm: 6, maxTerm: 48);

        _currentUserService.Setup(s => s.UserId).Returns(staffUserId);
        _currentUserService.Setup(s => s.IsBankManager).Returns(false);
        _currentUserService.Setup(s => s.IsSuperAdmin).Returns(false);
        _applicationUserRepository
            .Setup(r => r.GetAsync(
                It.IsAny<Expression<Func<ApplicationUser, bool>>>(),
                It.IsAny<Func<IQueryable<ApplicationUser>, IIncludableQueryable<ApplicationUser, object>>>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApplicationUser
            {
                Id = staffUserId,
                Email = "analyst@bank.local",
                Position = StaffPosition.CreditAnalyst
            });

        ApproveCreditApplicationRequest request = new()
        {
            Id = applicationId,
            ApprovedAmount = 25_000,
            ApprovedTermMonths = 24,
            ApprovedInterestRate = 1.9m
        };

        Func<Task> act = () => CreateSut().Handle(request, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessException>()
            .WithMessage(CreditApplicationMessages.AmountExceedsApprovalAuthority);
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

        Guid staffUserId = Guid.NewGuid();
        _currentUserService.Setup(s => s.UserId).Returns(staffUserId);
        _applicationUserRepository
            .Setup(r => r.GetAsync(
                It.IsAny<Expression<Func<ApplicationUser, bool>>>(),
                It.IsAny<Func<IQueryable<ApplicationUser>, IIncludableQueryable<ApplicationUser, object>>>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApplicationUser
            {
                Id = staffUserId,
                Email = "staff@bank.local"
            });

        _creditApplicationRepository
            .Setup(r => r.UpdateAsync(It.IsAny<CreditApplication>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CreditApplication entity, CancellationToken _) => entity);

        ApproveCreditApplicationRequest request = new()
        {
            Id = applicationId,
            ApprovedAmount = 25_000,
            ApprovedTermMonths = 24,
            ApprovedInterestRate = 1.9m,
            Notes = "ok"
        };

        ApprovedCreditApplicationResponse response = await CreateSut().Handle(request, CancellationToken.None);

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

    private void SetupDocuments(IList<CreditApplicationDocument> docs)
    {
        _documentRepository
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
                Items = docs,
                Index = 0,
                Size = 50,
                Count = docs.Count,
                Pages = 1
            });
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
