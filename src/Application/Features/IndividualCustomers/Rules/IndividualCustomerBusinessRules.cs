using Application.Features.IndividualCustomers.Constants;
using Application.Services.Repositories;
using Core.Application.Pipelines;
using Core.CrossCuttingConcerns.Exceptions.Types;
using Domain.Entities;

namespace Application.Features.IndividualCustomers.Rules;

public class IndividualCustomerBusinessRules : BaseBusinessRules
{
    private readonly IIndividualCustomerRepository _individualCustomerRepository;
    private readonly ICustomerRepository _customerRepository;

    public IndividualCustomerBusinessRules(
        IIndividualCustomerRepository individualCustomerRepository,
        ICustomerRepository customerRepository)
    {
        _individualCustomerRepository = individualCustomerRepository;
        _customerRepository = customerRepository;
    }

    public Task IndividualCustomerShouldExistWhenSelected(IndividualCustomer? individualCustomer)
    {
        if (individualCustomer is null)
            throw new BusinessException(IndividualCustomerMessages.NotExists);

        return Task.CompletedTask;
    }

    public async Task NationalIdShouldBeUnique(string nationalId, Guid? excludeId = null)
    {
        bool exists = await _individualCustomerRepository.AnyAsync(
            c => c.NationalId == nationalId && (!excludeId.HasValue || c.Id != excludeId.Value));

        if (exists)
            throw new BusinessException(IndividualCustomerMessages.NationalIdAlreadyExists);
    }

    public async Task CustomerNumberShouldBeUnique(string customerNumber, Guid? excludeId = null)
    {
        bool exists = await _customerRepository.AnyAsync(
            c => c.CustomerNumber == customerNumber && (!excludeId.HasValue || c.Id != excludeId.Value));

        if (exists)
            throw new BusinessException(IndividualCustomerMessages.CustomerNumberAlreadyExists);
    }

    public async Task EmailShouldBeUnique(string email, Guid? excludeId = null)
    {
        bool exists = await _customerRepository.AnyAsync(
            c => c.Email == email && (!excludeId.HasValue || c.Id != excludeId.Value));

        if (exists)
            throw new BusinessException(IndividualCustomerMessages.EmailAlreadyExists);
    }
}
