using Application.Modules.Credit.Features.CreditApplications.Constants;
using Application.Services.Repositories;
using Core.Application.Pipelines;
using Core.CrossCuttingConcerns.Exceptions.Types;
using Domain.CreditPackaging;
using Domain.Entities;
using Domain.Enums;

namespace Application.Modules.Credit.Features.CreditApplications.Rules;

public class CreditApplicationBusinessRules : BaseBusinessRules
{
    private readonly ICreditApplicationRepository _creditApplicationRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IIndividualCustomerRepository _individualCustomerRepository;
    private readonly ICorporateCustomerRepository _corporateCustomerRepository;
    private readonly ICreditSubTypeRepository _creditSubTypeRepository;
    private readonly ICreditTypeRepository _creditTypeRepository;

    public CreditApplicationBusinessRules(
        ICreditApplicationRepository creditApplicationRepository,
        ICustomerRepository customerRepository,
        IIndividualCustomerRepository individualCustomerRepository,
        ICorporateCustomerRepository corporateCustomerRepository,
        ICreditSubTypeRepository creditSubTypeRepository,
        ICreditTypeRepository creditTypeRepository)
    {
        _creditApplicationRepository = creditApplicationRepository;
        _customerRepository = customerRepository;
        _individualCustomerRepository = individualCustomerRepository;
        _corporateCustomerRepository = corporateCustomerRepository;
        _creditSubTypeRepository = creditSubTypeRepository;
        _creditTypeRepository = creditTypeRepository;
    }

    public Task CreditApplicationShouldExistWhenSelected(CreditApplication? creditApplication)
    {
        if (creditApplication is null)
            throw new BusinessException(CreditApplicationMessages.NotExists);

        return Task.CompletedTask;
    }

    public async Task CustomerShouldExistAndBeActive(Guid customerId)
    {
        Customer? customer = await _customerRepository.GetAsync(c => c.Id == customerId);
        if (customer is null)
            throw new BusinessException(CreditApplicationMessages.CustomerNotExists);

        if (!customer.IsActive)
            throw new BusinessException(CreditApplicationMessages.CustomerNotActive);
    }

    public async Task CreditSubTypeShouldExistAndBeActive(Guid creditSubTypeId)
    {
        CreditSubType? creditSubType = await _creditSubTypeRepository.GetAsync(c => c.Id == creditSubTypeId);
        if (creditSubType is null)
            throw new BusinessException(CreditApplicationMessages.CreditSubTypeNotExists);

        if (!creditSubType.IsActive)
            throw new BusinessException(CreditApplicationMessages.CreditSubTypeNotActive);

        CreditType? creditType = await _creditTypeRepository.GetAsync(c => c.Id == creditSubType.CreditTypeId);
        if (creditType is null)
            throw new BusinessException(CreditApplicationMessages.CreditSubTypeNotExists);

        if (!creditType.IsActive)
            throw new BusinessException(CreditApplicationMessages.CreditTypeNotActive);
    }

    public async Task CustomerCategoryShouldMatchCreditSubType(Guid customerId, Guid creditSubTypeId)
    {
        CreditSubType? creditSubType = await _creditSubTypeRepository.GetAsync(c => c.Id == creditSubTypeId);
        if (creditSubType is null)
            throw new BusinessException(CreditApplicationMessages.CreditSubTypeNotExists);

        CreditType? creditType = await _creditTypeRepository.GetAsync(c => c.Id == creditSubType.CreditTypeId);
        if (creditType is null)
            throw new BusinessException(CreditApplicationMessages.CreditSubTypeNotExists);

        bool isIndividual = await _individualCustomerRepository.AnyAsync(c => c.Id == customerId);
        bool isCorporate = await _corporateCustomerRepository.AnyAsync(c => c.Id == customerId);

        bool mismatch =
            (isIndividual && creditType.Category != CreditCategory.Individual) ||
            (isCorporate && creditType.Category != CreditCategory.Corporate) ||
            (!isIndividual && !isCorporate);

        if (mismatch)
            throw new BusinessException(CreditApplicationMessages.CategoryMismatch);
    }

    public async Task RequestShouldBeWithinCreditSubTypeLimits(
        Guid creditSubTypeId,
        decimal requestedAmount,
        int requestedTermMonths)
    {
        CreditSubType? creditSubType = await _creditSubTypeRepository.GetAsync(c => c.Id == creditSubTypeId);
        if (creditSubType is null)
            throw new BusinessException(CreditApplicationMessages.CreditSubTypeNotExists);

        if (requestedAmount < creditSubType.MinAmount || requestedAmount > creditSubType.MaxAmount)
            throw new BusinessException(CreditApplicationMessages.AmountOutOfRange);

        CreditType? creditType = await _creditTypeRepository.GetAsync(c => c.Id == creditSubType.CreditTypeId);
        string currency = creditType?.CurrencyCode ?? "USD";

        if (!CreditPackageConstraints.IsAllowedAmount(
                requestedAmount,
                creditSubType.MinAmount,
                creditSubType.MaxAmount,
                currency))
            throw new BusinessException(CreditApplicationMessages.AmountNotOnPackageGrid);

        if (requestedTermMonths < creditSubType.MinTermMonths || requestedTermMonths > creditSubType.MaxTermMonths)
            throw new BusinessException(CreditApplicationMessages.TermOutOfRange);

        if (!CreditPackageConstraints.IsAllowedTerm(
                requestedTermMonths,
                creditSubType.MinTermMonths,
                creditSubType.MaxTermMonths))
            throw new BusinessException(CreditApplicationMessages.TermNotAllowed);
    }

    public async Task ApplicationNumberShouldBeUnique(string applicationNumber, Guid? excludeId = null)
    {
        bool exists = await _creditApplicationRepository.AnyAsync(
            c => c.ApplicationNumber == applicationNumber && (!excludeId.HasValue || c.Id != excludeId.Value));

        if (exists)
            throw new BusinessException(CreditApplicationMessages.ApplicationNumberAlreadyExists);
    }

    public Task ApplicationShouldBePending(CreditApplication creditApplication)
    {
        if (creditApplication.Status != CreditApplicationStatus.Pending)
            throw new BusinessException(CreditApplicationMessages.OnlyPendingCanBeDecided);

        return Task.CompletedTask;
    }

    public Task ApplicationShouldBePendingForUpdate(CreditApplication creditApplication)
    {
        if (creditApplication.Status != CreditApplicationStatus.Pending)
            throw new BusinessException(CreditApplicationMessages.OnlyPendingCanBeUpdated);

        return Task.CompletedTask;
    }

    public Task ApplicationShouldBePendingForCancel(CreditApplication creditApplication)
    {
        if (creditApplication.Status != CreditApplicationStatus.Pending)
            throw new BusinessException(CreditApplicationMessages.OnlyPendingCanBeCancelled);

        return Task.CompletedTask;
    }
}
