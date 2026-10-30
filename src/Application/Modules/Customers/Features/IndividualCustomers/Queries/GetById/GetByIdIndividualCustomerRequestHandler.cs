using Application.Modules.Customers.Features.IndividualCustomers.Rules;
using Application.Services.Auth;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;

namespace Application.Modules.Customers.Features.IndividualCustomers.Queries.GetById;

public class GetByIdIndividualCustomerRequestHandler
    : IRequestHandler<GetByIdIndividualCustomerRequest, GetByIdIndividualCustomerResponse>
{
    private readonly IIndividualCustomerRepository _individualCustomerRepository;
    private readonly IMapper _mapper;
    private readonly IndividualCustomerBusinessRules _businessRules;
    private readonly IBranchAuthorizationService _branchAuthorization;

    public GetByIdIndividualCustomerRequestHandler(
        IIndividualCustomerRepository individualCustomerRepository,
        IMapper mapper,
        IndividualCustomerBusinessRules businessRules,
        IBranchAuthorizationService branchAuthorization)
    {
        _individualCustomerRepository = individualCustomerRepository;
        _mapper = mapper;
        _businessRules = businessRules;
        _branchAuthorization = branchAuthorization;
    }

    public async Task<GetByIdIndividualCustomerResponse> Handle(
        GetByIdIndividualCustomerRequest request,
        CancellationToken cancellationToken)
    {
        await _branchAuthorization.EnsureCanViewCustomerAsync(request.Id, cancellationToken);

        IndividualCustomer? individualCustomer = await _individualCustomerRepository.GetAsync(
            predicate: c => c.Id == request.Id,
            cancellationToken: cancellationToken);

        await _businessRules.IndividualCustomerShouldExistWhenSelected(individualCustomer);

        return _mapper.Map<GetByIdIndividualCustomerResponse>(individualCustomer);
    }
}
