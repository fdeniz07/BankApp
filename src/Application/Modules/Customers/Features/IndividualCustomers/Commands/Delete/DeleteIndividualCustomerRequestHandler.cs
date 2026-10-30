using Application.Modules.Customers.Features.Customers.Rules;
using Application.Modules.Customers.Features.IndividualCustomers.Rules;
using Application.Services.Auth;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;

namespace Application.Modules.Customers.Features.IndividualCustomers.Commands.Delete;

public class DeleteIndividualCustomerRequestHandler
    : IRequestHandler<DeleteIndividualCustomerRequest, DeletedIndividualCustomerResponse>
{
    private readonly IIndividualCustomerRepository _individualCustomerRepository;
    private readonly IApplicationUserRepository _applicationUserRepository;
    private readonly IMapper _mapper;
    private readonly IndividualCustomerBusinessRules _businessRules;
    private readonly CustomerLifecycleBusinessRules _lifecycleRules;
    private readonly ICurrentUserService _currentUserService;
    private readonly IBranchAuthorizationService _branchAuthorization;

    public DeleteIndividualCustomerRequestHandler(
        IIndividualCustomerRepository individualCustomerRepository,
        IApplicationUserRepository applicationUserRepository,
        IMapper mapper,
        IndividualCustomerBusinessRules businessRules,
        CustomerLifecycleBusinessRules lifecycleRules,
        ICurrentUserService currentUserService,
        IBranchAuthorizationService branchAuthorization)
    {
        _individualCustomerRepository = individualCustomerRepository;
        _applicationUserRepository = applicationUserRepository;
        _mapper = mapper;
        _businessRules = businessRules;
        _lifecycleRules = lifecycleRules;
        _currentUserService = currentUserService;
        _branchAuthorization = branchAuthorization;
    }

    public async Task<DeletedIndividualCustomerResponse> Handle(
        DeleteIndividualCustomerRequest request,
        CancellationToken cancellationToken)
    {
        _currentUserService.EnsureBankStaff();
        await _branchAuthorization.EnsureCanModifyCustomerAsync(request.Id, cancellationToken);

        IndividualCustomer? individualCustomer = await _individualCustomerRepository.GetAsync(
            predicate: c => c.Id == request.Id,
            cancellationToken: cancellationToken);

        await _businessRules.IndividualCustomerShouldExistWhenSelected(individualCustomer);
        await _lifecycleRules.CustomerShouldBeInactiveBeforeDelete(individualCustomer!);
        await _lifecycleRules.CustomerShouldNotHaveOpenCreditApplications(individualCustomer!.Id);

        ApplicationUser? applicationUser = await _applicationUserRepository.GetAsync(
            u => u.CustomerId == request.Id,
            cancellationToken: cancellationToken);

        if (applicationUser is not null)
        {
            applicationUser.IsActive = false;
            await _applicationUserRepository.DeleteAsync(applicationUser, cancellationToken: cancellationToken);
        }

        individualCustomer.IsActive = false;
        await _individualCustomerRepository.DeleteAsync(individualCustomer, cancellationToken: cancellationToken);

        return _mapper.Map<DeletedIndividualCustomerResponse>(individualCustomer);
    }
}
