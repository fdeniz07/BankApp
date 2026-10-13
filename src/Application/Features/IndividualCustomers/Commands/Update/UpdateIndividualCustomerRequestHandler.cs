using Application.Features.IndividualCustomers.Rules;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;

namespace Application.Features.IndividualCustomers.Commands.Update;

public class UpdateIndividualCustomerRequestHandler
    : IRequestHandler<UpdateIndividualCustomerRequest, UpdatedIndividualCustomerResponse>
{
    private readonly IIndividualCustomerRepository _individualCustomerRepository;
    private readonly IMapper _mapper;
    private readonly IndividualCustomerBusinessRules _businessRules;

    public UpdateIndividualCustomerRequestHandler(
        IIndividualCustomerRepository individualCustomerRepository,
        IMapper mapper,
        IndividualCustomerBusinessRules businessRules)
    {
        _individualCustomerRepository = individualCustomerRepository;
        _mapper = mapper;
        _businessRules = businessRules;
    }

    public async Task<UpdatedIndividualCustomerResponse> Handle(
        UpdateIndividualCustomerRequest request,
        CancellationToken cancellationToken)
    {
        IndividualCustomer? individualCustomer = await _individualCustomerRepository.GetAsync(
            predicate: c => c.Id == request.Id,
            cancellationToken: cancellationToken);

        await _businessRules.IndividualCustomerShouldExistWhenSelected(individualCustomer);
        await _businessRules.CustomerNumberShouldBeUnique(request.CustomerNumber, request.Id);
        await _businessRules.EmailShouldBeUnique(request.Email, request.Id);
        await _businessRules.NationalIdShouldBeUnique(request.NationalId, request.Id);

        individualCustomer = _mapper.Map(request, individualCustomer);
        await _individualCustomerRepository.UpdateAsync(individualCustomer!, cancellationToken);

        return _mapper.Map<UpdatedIndividualCustomerResponse>(individualCustomer);
    }
}
