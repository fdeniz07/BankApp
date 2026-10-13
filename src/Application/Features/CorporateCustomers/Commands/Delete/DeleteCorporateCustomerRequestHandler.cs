using Application.Features.CorporateCustomers.Rules;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;

namespace Application.Features.CorporateCustomers.Commands.Delete;

public class DeleteCorporateCustomerRequestHandler
    : IRequestHandler<DeleteCorporateCustomerRequest, DeletedCorporateCustomerResponse>
{
    private readonly ICorporateCustomerRepository _corporateCustomerRepository;
    private readonly IMapper _mapper;
    private readonly CorporateCustomerBusinessRules _businessRules;

    public DeleteCorporateCustomerRequestHandler(
        ICorporateCustomerRepository corporateCustomerRepository,
        IMapper mapper,
        CorporateCustomerBusinessRules businessRules)
    {
        _corporateCustomerRepository = corporateCustomerRepository;
        _mapper = mapper;
        _businessRules = businessRules;
    }

    public async Task<DeletedCorporateCustomerResponse> Handle(
        DeleteCorporateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        CorporateCustomer? corporateCustomer = await _corporateCustomerRepository.GetAsync(
            predicate: c => c.Id == request.Id,
            cancellationToken: cancellationToken);

        await _businessRules.CorporateCustomerShouldExistWhenSelected(corporateCustomer);

        corporateCustomer!.IsActive = false;
        await _corporateCustomerRepository.DeleteAsync(corporateCustomer, cancellationToken: cancellationToken);

        return _mapper.Map<DeletedCorporateCustomerResponse>(corporateCustomer);
    }
}
