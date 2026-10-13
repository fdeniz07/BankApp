using Application.Features.CorporateCustomers.Rules;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;

namespace Application.Features.CorporateCustomers.Commands.Create;

public class CreateCorporateCustomerRequestHandler
    : IRequestHandler<CreateCorporateCustomerRequest, CreatedCorporateCustomerResponse>
{
    private readonly ICorporateCustomerRepository _corporateCustomerRepository;
    private readonly IMapper _mapper;
    private readonly CorporateCustomerBusinessRules _businessRules;

    public CreateCorporateCustomerRequestHandler(
        ICorporateCustomerRepository corporateCustomerRepository,
        IMapper mapper,
        CorporateCustomerBusinessRules businessRules)
    {
        _corporateCustomerRepository = corporateCustomerRepository;
        _mapper = mapper;
        _businessRules = businessRules;
    }

    public async Task<CreatedCorporateCustomerResponse> Handle(
        CreateCorporateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        await _businessRules.CustomerNumberShouldBeUnique(request.CustomerNumber);
        await _businessRules.EmailShouldBeUnique(request.Email);
        await _businessRules.TaxNumberShouldBeUnique(request.TaxNumber);
        await _businessRules.CompanyRegistrationNumberShouldBeUnique(request.CompanyRegistrationNumber);

        CorporateCustomer corporateCustomer = _mapper.Map<CorporateCustomer>(request);
        await _corporateCustomerRepository.AddAsync(corporateCustomer, cancellationToken);

        return _mapper.Map<CreatedCorporateCustomerResponse>(corporateCustomer);
    }
}
