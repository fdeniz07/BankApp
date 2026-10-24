using Application.Modules.Customers.Features.Customers.Constants;
using Application.Services.Repositories;
using Core.Application.Pipelines;
using Core.CrossCuttingConcerns.Exceptions.Types;
using Domain.Entities;
using Domain.Enums;

namespace Application.Modules.Customers.Features.Customers.Rules;

public class CustomerLifecycleBusinessRules : BaseBusinessRules
{
    private readonly ICreditApplicationRepository _creditApplicationRepository;

    public CustomerLifecycleBusinessRules(ICreditApplicationRepository creditApplicationRepository)
    {
        _creditApplicationRepository = creditApplicationRepository;
    }

    public Task CustomerShouldBeInactiveBeforeDelete(Customer customer)
    {
        if (customer.IsActive)
            throw new BusinessException(CustomerLifecycleMessages.MustBeInactiveBeforeDelete);

        return Task.CompletedTask;
    }

    public async Task CustomerShouldNotHaveOpenCreditApplications(Guid customerId)
    {
        bool hasOpen = await _creditApplicationRepository.AnyAsync(a =>
            a.CustomerId == customerId &&
            !a.IsDeleted &&
            (a.Status == CreditApplicationStatus.Pending || a.Status == CreditApplicationStatus.Approved));

        if (hasOpen)
            throw new BusinessException(CustomerLifecycleMessages.HasOpenCreditApplications);
    }

    public Task GenderShouldBeValid(string? gender)
    {
        if (!Gender.IsValidName(gender))
            throw new BusinessException(CustomerLifecycleMessages.InvalidGender);

        return Task.CompletedTask;
    }

    public Task GenderShouldBeValidWhenProvided(string? gender)
    {
        if (string.IsNullOrWhiteSpace(gender))
            return Task.CompletedTask;

        return GenderShouldBeValid(gender);
    }
}
