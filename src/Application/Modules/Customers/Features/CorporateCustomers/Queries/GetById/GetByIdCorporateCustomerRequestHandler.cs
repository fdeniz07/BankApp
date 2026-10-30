using Application.Modules.Customers.Features.CorporateCustomers.Rules;
using Application.Services.Auth;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;

namespace Application.Modules.Customers.Features.CorporateCustomers.Queries.GetById;

public class GetByIdCorporateCustomerRequestHandler
    : IRequestHandler<GetByIdCorporateCustomerRequest, GetByIdCorporateCustomerResponse>
{
    private readonly ICorporateCustomerRepository _corporateCustomerRepository;
    private readonly IMapper _mapper;
    private readonly CorporateCustomerBusinessRules _businessRules;
    private readonly IBranchAuthorizationService _branchAuthorization;

    public GetByIdCorporateCustomerRequestHandler(
        ICorporateCustomerRepository corporateCustomerRepository,
        IMapper mapper,
        CorporateCustomerBusinessRules businessRules,
        IBranchAuthorizationService branchAuthorization)
    {
        _corporateCustomerRepository = corporateCustomerRepository;
        _mapper = mapper;
        _businessRules = businessRules;
        _branchAuthorization = branchAuthorization;
    }

    public async Task<GetByIdCorporateCustomerResponse> Handle(
        GetByIdCorporateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        await _branchAuthorization.EnsureCanViewCustomerAsync(request.Id, cancellationToken);

        CorporateCustomer? corporateCustomer = await _corporateCustomerRepository.GetAsync(
            predicate: c => c.Id == request.Id,
            cancellationToken: cancellationToken);

        await _businessRules.CorporateCustomerShouldExistWhenSelected(corporateCustomer);

        return _mapper.Map<GetByIdCorporateCustomerResponse>(corporateCustomer);
    }
}
