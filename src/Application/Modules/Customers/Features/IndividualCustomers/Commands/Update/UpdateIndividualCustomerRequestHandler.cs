using Application.Modules.Identity.Features.Auth.Rules;
using Application.Modules.Customers.Features.Customers.Rules;
using Application.Modules.Customers.Features.IndividualCustomers.Rules;
using Application.Services.Auth;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using MediatR;

namespace Application.Modules.Customers.Features.IndividualCustomers.Commands.Update;

public class UpdateIndividualCustomerRequestHandler
    : IRequestHandler<UpdateIndividualCustomerRequest, UpdatedIndividualCustomerResponse>
{
    private readonly IIndividualCustomerRepository _individualCustomerRepository;
    private readonly IApplicationUserRepository _applicationUserRepository;
    private readonly IMapper _mapper;
    private readonly IndividualCustomerBusinessRules _businessRules;
    private readonly AuthBusinessRules _authBusinessRules;
    private readonly CustomerLifecycleBusinessRules _lifecycleRules;
    private readonly IBranchAuthorizationService _branchAuthorization;

    public UpdateIndividualCustomerRequestHandler(
        IIndividualCustomerRepository individualCustomerRepository,
        IApplicationUserRepository applicationUserRepository,
        IMapper mapper,
        IndividualCustomerBusinessRules businessRules,
        AuthBusinessRules authBusinessRules,
        CustomerLifecycleBusinessRules lifecycleRules,
        IBranchAuthorizationService branchAuthorization)
    {
        _individualCustomerRepository = individualCustomerRepository;
        _applicationUserRepository = applicationUserRepository;
        _mapper = mapper;
        _businessRules = businessRules;
        _authBusinessRules = authBusinessRules;
        _lifecycleRules = lifecycleRules;
        _branchAuthorization = branchAuthorization;
    }

    public async Task<UpdatedIndividualCustomerResponse> Handle(
        UpdateIndividualCustomerRequest request,
        CancellationToken cancellationToken)
    {
        await _branchAuthorization.EnsureCanModifyCustomerAsync(request.Id, cancellationToken);

        IndividualCustomer? individualCustomer = await _individualCustomerRepository.GetAsync(
            predicate: c => c.Id == request.Id,
            cancellationToken: cancellationToken);

        await _businessRules.IndividualCustomerShouldExistWhenSelected(individualCustomer);
        // CustomerNumber is system-assigned and immutable.
        await _businessRules.EmailShouldBeUnique(request.Email, request.Id);
        await _businessRules.NationalIdShouldBeUnique(request.NationalId, request.Id);
        await _lifecycleRules.GenderShouldBeValidWhenProvided(request.Gender);
        if (!string.IsNullOrWhiteSpace(request.Gender))
            request.Gender = Gender.Normalize(request.Gender);

        ApplicationUser? applicationUser = await _applicationUserRepository.GetAsync(
            u => u.CustomerId == request.Id,
            cancellationToken: cancellationToken);

        if (applicationUser is not null &&
            !string.Equals(applicationUser.Email, request.Email, StringComparison.OrdinalIgnoreCase))
        {
            await _authBusinessRules.EmailShouldBeUnique(request.Email, applicationUser.Id);
            applicationUser.Email = request.Email;
            await _applicationUserRepository.UpdateAsync(applicationUser, cancellationToken);
        }

        individualCustomer = _mapper.Map(request, individualCustomer);
        await _individualCustomerRepository.UpdateAsync(individualCustomer!, cancellationToken);

        return _mapper.Map<UpdatedIndividualCustomerResponse>(individualCustomer);
    }
}
