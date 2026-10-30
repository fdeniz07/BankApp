using Application.Modules.Customers.Features.CorporateCustomers.Rules;
using Application.Modules.Customers.Features.Customers.Rules;
using Application.Services.Auth;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;

namespace Application.Modules.Customers.Features.CorporateCustomers.Commands.Delete;

public class DeleteCorporateCustomerRequestHandler
    : IRequestHandler<DeleteCorporateCustomerRequest, DeletedCorporateCustomerResponse>
{
    private readonly ICorporateCustomerRepository _corporateCustomerRepository;
    private readonly IApplicationUserRepository _applicationUserRepository;
    private readonly IMapper _mapper;
    private readonly CorporateCustomerBusinessRules _businessRules;
    private readonly CustomerLifecycleBusinessRules _lifecycleRules;
    private readonly ICurrentUserService _currentUserService;
    private readonly IBranchAuthorizationService _branchAuthorization;

    public DeleteCorporateCustomerRequestHandler(
        ICorporateCustomerRepository corporateCustomerRepository,
        IApplicationUserRepository applicationUserRepository,
        IMapper mapper,
        CorporateCustomerBusinessRules businessRules,
        CustomerLifecycleBusinessRules lifecycleRules,
        ICurrentUserService currentUserService,
        IBranchAuthorizationService branchAuthorization)
    {
        _corporateCustomerRepository = corporateCustomerRepository;
        _applicationUserRepository = applicationUserRepository;
        _mapper = mapper;
        _businessRules = businessRules;
        _lifecycleRules = lifecycleRules;
        _currentUserService = currentUserService;
        _branchAuthorization = branchAuthorization;
    }

    public async Task<DeletedCorporateCustomerResponse> Handle(
        DeleteCorporateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        _currentUserService.EnsureBankStaff();
        await _branchAuthorization.EnsureCanModifyCustomerAsync(request.Id, cancellationToken);

        CorporateCustomer? corporateCustomer = await _corporateCustomerRepository.GetAsync(
            predicate: c => c.Id == request.Id,
            cancellationToken: cancellationToken);

        await _businessRules.CorporateCustomerShouldExistWhenSelected(corporateCustomer);
        await _lifecycleRules.CustomerShouldBeInactiveBeforeDelete(corporateCustomer!);
        await _lifecycleRules.CustomerShouldNotHaveOpenCreditApplications(corporateCustomer!.Id);

        ApplicationUser? applicationUser = await _applicationUserRepository.GetAsync(
            u => u.CustomerId == request.Id,
            cancellationToken: cancellationToken);

        if (applicationUser is not null)
        {
            applicationUser.IsActive = false;
            await _applicationUserRepository.DeleteAsync(applicationUser, cancellationToken: cancellationToken);
        }

        corporateCustomer.IsActive = false;
        await _corporateCustomerRepository.DeleteAsync(corporateCustomer, cancellationToken: cancellationToken);

        return _mapper.Map<DeletedCorporateCustomerResponse>(corporateCustomer);
    }
}
