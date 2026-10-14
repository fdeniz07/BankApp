using Application.Features.CorporateCustomers.Rules;
using Application.Services.Auth;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;

namespace Application.Features.CorporateCustomers.Commands.Update;

public class UpdateCorporateCustomerRequestHandler
    : IRequestHandler<UpdateCorporateCustomerRequest, UpdatedCorporateCustomerResponse>
{
    private readonly ICorporateCustomerRepository _corporateCustomerRepository;
    private readonly IMapper _mapper;
    private readonly CorporateCustomerBusinessRules _businessRules;
    private readonly ICurrentUserService _currentUserService;

    public UpdateCorporateCustomerRequestHandler(
        ICorporateCustomerRepository corporateCustomerRepository,
        IMapper mapper,
        CorporateCustomerBusinessRules businessRules,
        ICurrentUserService currentUserService)
    {
        _corporateCustomerRepository = corporateCustomerRepository;
        _mapper = mapper;
        _businessRules = businessRules;
        _currentUserService = currentUserService;
    }

    public async Task<UpdatedCorporateCustomerResponse> Handle(
        UpdateCorporateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        _currentUserService.EnsureCanAccessCustomer(request.Id);

        CorporateCustomer? corporateCustomer = await _corporateCustomerRepository.GetAsync(
            predicate: c => c.Id == request.Id,
            cancellationToken: cancellationToken);

        await _businessRules.CorporateCustomerShouldExistWhenSelected(corporateCustomer);
        await _businessRules.CustomerNumberShouldBeUnique(request.CustomerNumber, request.Id);
        await _businessRules.EmailShouldBeUnique(request.Email, request.Id);
        await _businessRules.TaxNumberShouldBeUnique(request.TaxNumber, request.Id);
        await _businessRules.CompanyRegistrationNumberShouldBeUnique(request.CompanyRegistrationNumber, request.Id);

        corporateCustomer = _mapper.Map(request, corporateCustomer);
        await _corporateCustomerRepository.UpdateAsync(corporateCustomer!, cancellationToken);

        return _mapper.Map<UpdatedCorporateCustomerResponse>(corporateCustomer);
    }
}
