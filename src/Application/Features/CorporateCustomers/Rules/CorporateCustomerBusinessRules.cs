using Application.Features.CorporateCustomers.Constants;
using Application.Services.Repositories;
using Core.Application.Pipelines;
using Core.CrossCuttingConcerns.Exceptions.Types;
using Domain.Entities;

namespace Application.Features.CorporateCustomers.Rules;

public class CorporateCustomerBusinessRules : BaseBusinessRules
{
    private readonly ICorporateCustomerRepository _corporateCustomerRepository;
    private readonly ICustomerRepository _customerRepository;

    public CorporateCustomerBusinessRules(
        ICorporateCustomerRepository corporateCustomerRepository,
        ICustomerRepository customerRepository)
    {
        _corporateCustomerRepository = corporateCustomerRepository;
        _customerRepository = customerRepository;
    }

    public Task CorporateCustomerShouldExistWhenSelected(CorporateCustomer? corporateCustomer)
    {
        if (corporateCustomer is null)
            throw new BusinessException(CorporateCustomerMessages.NotExists);

        return Task.CompletedTask;
    }

    public async Task TaxNumberShouldBeUnique(string taxNumber, Guid? excludeId = null)
    {
        bool exists = await _corporateCustomerRepository.AnyAsync(
            c => c.TaxNumber == taxNumber && (!excludeId.HasValue || c.Id != excludeId.Value));

        if (exists)
            throw new BusinessException(CorporateCustomerMessages.TaxNumberAlreadyExists);
    }

    public async Task CompanyRegistrationNumberShouldBeUnique(string registrationNumber, Guid? excludeId = null)
    {
        bool exists = await _corporateCustomerRepository.AnyAsync(
            c => c.CompanyRegistrationNumber == registrationNumber
                 && (!excludeId.HasValue || c.Id != excludeId.Value));

        if (exists)
            throw new BusinessException(CorporateCustomerMessages.CompanyRegistrationNumberAlreadyExists);
    }

    public async Task CustomerNumberShouldBeUnique(string customerNumber, Guid? excludeId = null)
    {
        bool exists = await _customerRepository.AnyAsync(
            c => c.CustomerNumber == customerNumber && (!excludeId.HasValue || c.Id != excludeId.Value));

        if (exists)
            throw new BusinessException(CorporateCustomerMessages.CustomerNumberAlreadyExists);
    }

    public async Task EmailShouldBeUnique(string email, Guid? excludeId = null)
    {
        bool exists = await _customerRepository.AnyAsync(
            c => c.Email == email && (!excludeId.HasValue || c.Id != excludeId.Value));

        if (exists)
            throw new BusinessException(CorporateCustomerMessages.EmailAlreadyExists);
    }
}
