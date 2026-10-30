using Application.Services.Auth;
using Application.Services.Repositories;
using Domain.Enums;
using MediatR;

namespace Application.Modules.Credit.Features.Dashboard.Queries.GetSummary;

public class GetDashboardSummaryRequestHandler
    : IRequestHandler<GetDashboardSummaryRequest, GetDashboardSummaryResponse>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IIndividualCustomerRepository _individualCustomerRepository;
    private readonly ICorporateCustomerRepository _corporateCustomerRepository;
    private readonly ICreditTypeRepository _creditTypeRepository;
    private readonly ICreditApplicationRepository _creditApplicationRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IBranchAuthorizationService _branchAuthorization;

    public GetDashboardSummaryRequestHandler(
        ICustomerRepository customerRepository,
        IIndividualCustomerRepository individualCustomerRepository,
        ICorporateCustomerRepository corporateCustomerRepository,
        ICreditTypeRepository creditTypeRepository,
        ICreditApplicationRepository creditApplicationRepository,
        ICurrentUserService currentUserService,
        IBranchAuthorizationService branchAuthorization)
    {
        _customerRepository = customerRepository;
        _individualCustomerRepository = individualCustomerRepository;
        _corporateCustomerRepository = corporateCustomerRepository;
        _creditTypeRepository = creditTypeRepository;
        _creditApplicationRepository = creditApplicationRepository;
        _currentUserService = currentUserService;
        _branchAuthorization = branchAuthorization;
    }

    public async Task<GetDashboardSummaryResponse> Handle(
        GetDashboardSummaryRequest request,
        CancellationToken cancellationToken)
    {
        if (_currentUserService.IsBankStaff)
            return await BuildStaffSummaryAsync(cancellationToken);

        Guid customerId = _currentUserService.GetRequiredCustomerId();
        return await BuildCustomerSummaryAsync(customerId, cancellationToken);
    }

    private async Task<GetDashboardSummaryResponse> BuildStaffSummaryAsync(CancellationToken cancellationToken)
    {
        Guid? branchFilter = _branchAuthorization.MustSeeNoBranchData
            ? Guid.Empty
            : _branchAuthorization.ResolveListBranchFilter(requestedBranchId: null);

        int customerTotal = await _customerRepository.CountAsync(
            c => c.IsActive && (!branchFilter.HasValue || c.BranchId == branchFilter.Value),
            cancellationToken: cancellationToken);
        int individualActive = await _individualCustomerRepository.CountAsync(
            c => c.IsActive && (!branchFilter.HasValue || c.BranchId == branchFilter.Value),
            cancellationToken: cancellationToken);
        int corporateActive = await _corporateCustomerRepository.CountAsync(
            c => c.IsActive && (!branchFilter.HasValue || c.BranchId == branchFilter.Value),
            cancellationToken: cancellationToken);
        int customerInactive = await _customerRepository.CountAsync(
            c => !c.IsActive && (!branchFilter.HasValue || c.BranchId == branchFilter.Value),
            cancellationToken: cancellationToken);

        int creditTypeTotal = await _creditTypeRepository.CountAsync(
            c => c.IsActive,
            cancellationToken: cancellationToken);
        int creditTypeIndividual = await _creditTypeRepository.CountAsync(
            c => c.IsActive && c.Category == CreditCategory.Individual,
            cancellationToken: cancellationToken);
        int creditTypeCorporate = await _creditTypeRepository.CountAsync(
            c => c.IsActive && c.Category == CreditCategory.Corporate,
            cancellationToken: cancellationToken);

        ApplicationCounts apps = await CountApplicationsAsync(
            customerId: null,
            branchId: branchFilter,
            cancellationToken);

        return new GetDashboardSummaryResponse
        {
            Scope = "Staff",
            CustomerTotal = customerTotal,
            IndividualActive = individualActive,
            CorporateActive = corporateActive,
            CustomerInactive = customerInactive,
            CreditTypeTotal = creditTypeTotal,
            CreditTypeIndividual = creditTypeIndividual,
            CreditTypeCorporate = creditTypeCorporate,
            ApplicationTotal = apps.Total,
            ApplicationPending = apps.Pending,
            ApplicationApproved = apps.Approved,
            ApplicationRejected = apps.Rejected,
            ApplicationCancelled = apps.Cancelled
        };
    }

    private async Task<GetDashboardSummaryResponse> BuildCustomerSummaryAsync(
        Guid customerId,
        CancellationToken cancellationToken)
    {
        ApplicationCounts apps = await CountApplicationsAsync(customerId, branchId: null, cancellationToken);

        return new GetDashboardSummaryResponse
        {
            Scope = "Customer",
            ApplicationTotal = apps.Total,
            ApplicationPending = apps.Pending,
            ApplicationApproved = apps.Approved,
            ApplicationRejected = apps.Rejected,
            ApplicationCancelled = apps.Cancelled
        };
    }

    private async Task<ApplicationCounts> CountApplicationsAsync(
        Guid? customerId,
        Guid? branchId,
        CancellationToken cancellationToken)
    {
        int total = await _creditApplicationRepository.CountAsync(
            a => (!customerId.HasValue || a.CustomerId == customerId.Value)
                 && (!branchId.HasValue || a.BranchId == branchId.Value),
            cancellationToken: cancellationToken);
        int pending = await _creditApplicationRepository.CountAsync(
            a => (!customerId.HasValue || a.CustomerId == customerId.Value)
                 && (!branchId.HasValue || a.BranchId == branchId.Value)
                 && a.Status == CreditApplicationStatus.Pending,
            cancellationToken: cancellationToken);
        int approved = await _creditApplicationRepository.CountAsync(
            a => (!customerId.HasValue || a.CustomerId == customerId.Value)
                 && (!branchId.HasValue || a.BranchId == branchId.Value)
                 && a.Status == CreditApplicationStatus.Approved,
            cancellationToken: cancellationToken);
        int rejected = await _creditApplicationRepository.CountAsync(
            a => (!customerId.HasValue || a.CustomerId == customerId.Value)
                 && (!branchId.HasValue || a.BranchId == branchId.Value)
                 && a.Status == CreditApplicationStatus.Rejected,
            cancellationToken: cancellationToken);
        int cancelled = await _creditApplicationRepository.CountAsync(
            a => (!customerId.HasValue || a.CustomerId == customerId.Value)
                 && (!branchId.HasValue || a.BranchId == branchId.Value)
                 && a.Status == CreditApplicationStatus.Cancelled,
            cancellationToken: cancellationToken);

        return new ApplicationCounts(total, pending, approved, rejected, cancelled);
    }

    private readonly record struct ApplicationCounts(
        int Total,
        int Pending,
        int Approved,
        int Rejected,
        int Cancelled);
}
