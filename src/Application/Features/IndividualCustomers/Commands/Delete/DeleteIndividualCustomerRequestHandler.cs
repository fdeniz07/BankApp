using Application.Features.IndividualCustomers.Rules;
using Application.Services.Auth;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;

namespace Application.Features.IndividualCustomers.Commands.Delete;

public class DeleteIndividualCustomerRequestHandler
    : IRequestHandler<DeleteIndividualCustomerRequest, DeletedIndividualCustomerResponse>
{
    private readonly IIndividualCustomerRepository _individualCustomerRepository;
    private readonly IApplicationUserRepository _applicationUserRepository;
    private readonly IMapper _mapper;
    private readonly IndividualCustomerBusinessRules _businessRules;
    private readonly ICurrentUserService _currentUserService;

    public DeleteIndividualCustomerRequestHandler(
        IIndividualCustomerRepository individualCustomerRepository,
        IApplicationUserRepository applicationUserRepository,
        IMapper mapper,
        IndividualCustomerBusinessRules businessRules,
        ICurrentUserService currentUserService)
    {
        _individualCustomerRepository = individualCustomerRepository;
        _applicationUserRepository = applicationUserRepository;
        _mapper = mapper;
        _businessRules = businessRules;
        _currentUserService = currentUserService;
    }

    public async Task<DeletedIndividualCustomerResponse> Handle(
        DeleteIndividualCustomerRequest request,
        CancellationToken cancellationToken)
    {
        _currentUserService.EnsureCanAccessCustomer(request.Id);

        IndividualCustomer? individualCustomer = await _individualCustomerRepository.GetAsync(
            predicate: c => c.Id == request.Id,
            cancellationToken: cancellationToken);

        await _businessRules.IndividualCustomerShouldExistWhenSelected(individualCustomer);

        ApplicationUser? applicationUser = await _applicationUserRepository.GetAsync(
            u => u.CustomerId == request.Id,
            cancellationToken: cancellationToken);

        if (applicationUser is not null)
        {
            applicationUser.IsActive = false;
            await _applicationUserRepository.DeleteAsync(applicationUser, cancellationToken: cancellationToken);
        }

        individualCustomer!.IsActive = false;
        await _individualCustomerRepository.DeleteAsync(individualCustomer, cancellationToken: cancellationToken);

        return _mapper.Map<DeletedIndividualCustomerResponse>(individualCustomer);
    }
}
