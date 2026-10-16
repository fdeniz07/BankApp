using Application.Services.Auth;
using Application.Services.Repositories;
using Domain.Enums;
using MediatR;

namespace Application.Features.Dashboard.Queries.GetSummary;

public class GetDashboardSummaryRequestHandler
    : IRequestHandler<GetDashboardSummaryRequest, GetDashboardSummaryResponse>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IIndividualCustomerRepository _individualCustomerRepository;
    private readonly ICorporateCustomerRepository _corporateCustomerRepository;
    private readonly ICreditApplicationRepository _creditApplicationRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetDashboardSummaryRequestHandler(
        ICustomerRepository customerRepository,
        IIndividualCustomerRepository individualCustomerRepository,
        ICorporateCustomerRepository corporateCustomerRepository,
        ICreditApplicationRepository creditApplicationRepository,
        ICurrentUserService currentUserService)
    {
        _customerRepository = customerRepository;
        _individualCustomerRepository = individualCustomerRepository;
        _corporateCustomerRepository = corporateCustomerRepository;
        _creditApplicationRepository = creditApplicationRepository;
        _currentUserService = currentUserService;
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
        int customerTotal = await _customerRepository.CountAsync(
            c => c.IsActive,
            cancellationToken: cancellationToken);
        int individualActive = await _individualCustomerRepository.CountAsync(
            c => c.IsActive,
            cancellationToken: cancellationToken);
        int corporateActive = await _corporateCustomerRepository.CountAsync(
            c => c.IsActive,
            cancellationToken: cancellationToken);
        int customerInactive = await _customerRepository.CountAsync(
            c => !c.IsActive,
            cancellationToken: cancellationToken);

        ApplicationCounts apps = await CountApplicationsAsync(customerId: null, cancellationToken);

        return new GetDashboardSummaryResponse
        {
            Scope = "Staff",
            CustomerTotal = customerTotal,
            IndividualActive = individualActive,
            CorporateActive = corporateActive,
            CustomerInactive = customerInactive,
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
        ApplicationCounts apps = await CountApplicationsAsync(customerId, cancellationToken);

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
        CancellationToken cancellationToken)
    {
        int total = await _creditApplicationRepository.CountAsync(
            a => !customerId.HasValue || a.CustomerId == customerId.Value,
            cancellationToken: cancellationToken);
        int pending = await _creditApplicationRepository.CountAsync(
            a => (!customerId.HasValue || a.CustomerId == customerId.Value) &&
                 a.Status == CreditApplicationStatus.Pending,
            cancellationToken: cancellationToken);
        int approved = await _creditApplicationRepository.CountAsync(
            a => (!customerId.HasValue || a.CustomerId == customerId.Value) &&
                 a.Status == CreditApplicationStatus.Approved,
            cancellationToken: cancellationToken);
        int rejected = await _creditApplicationRepository.CountAsync(
            a => (!customerId.HasValue || a.CustomerId == customerId.Value) &&
                 a.Status == CreditApplicationStatus.Rejected,
            cancellationToken: cancellationToken);
        int cancelled = await _creditApplicationRepository.CountAsync(
            a => (!customerId.HasValue || a.CustomerId == customerId.Value) &&
                 a.Status == CreditApplicationStatus.Cancelled,
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
