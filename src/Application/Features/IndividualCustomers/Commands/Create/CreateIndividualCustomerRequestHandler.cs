using Application.Features.IndividualCustomers.Rules;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;

namespace Application.Features.IndividualCustomers.Commands.Create;

public class CreateIndividualCustomerRequestHandler
    : IRequestHandler<CreateIndividualCustomerRequest, CreatedIndividualCustomerResponse>
{
    private readonly IIndividualCustomerRepository _individualCustomerRepository;
    private readonly IMapper _mapper;
    private readonly IndividualCustomerBusinessRules _businessRules;

    public CreateIndividualCustomerRequestHandler(
        IIndividualCustomerRepository individualCustomerRepository,
        IMapper mapper,
        IndividualCustomerBusinessRules businessRules)
    {
        _individualCustomerRepository = individualCustomerRepository;
        _mapper = mapper;
        _businessRules = businessRules;
    }

    public async Task<CreatedIndividualCustomerResponse> Handle(
        CreateIndividualCustomerRequest request,
        CancellationToken cancellationToken)
    {
        await _businessRules.CustomerNumberShouldBeUnique(request.CustomerNumber);
        await _businessRules.EmailShouldBeUnique(request.Email);
        await _businessRules.NationalIdShouldBeUnique(request.NationalId);

        IndividualCustomer individualCustomer = _mapper.Map<IndividualCustomer>(request);
        await _individualCustomerRepository.AddAsync(individualCustomer, cancellationToken);

        return _mapper.Map<CreatedIndividualCustomerResponse>(individualCustomer);
    }
}
