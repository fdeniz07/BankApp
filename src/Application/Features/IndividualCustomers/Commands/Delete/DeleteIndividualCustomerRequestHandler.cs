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
    private readonly IMapper _mapper;
    private readonly IndividualCustomerBusinessRules _businessRules;
    private readonly ICurrentUserService _currentUserService;

    public DeleteIndividualCustomerRequestHandler(
        IIndividualCustomerRepository individualCustomerRepository,
        IMapper mapper,
        IndividualCustomerBusinessRules businessRules,
        ICurrentUserService currentUserService)
    {
        _individualCustomerRepository = individualCustomerRepository;
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

        individualCustomer!.IsActive = false;
        await _individualCustomerRepository.DeleteAsync(individualCustomer, cancellationToken: cancellationToken);

        return _mapper.Map<DeletedIndividualCustomerResponse>(individualCustomer);
    }
}
