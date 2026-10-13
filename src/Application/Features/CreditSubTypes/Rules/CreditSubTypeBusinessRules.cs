using Application.Features.CreditSubTypes.Constants;
using Application.Services.Repositories;
using Core.Application.Pipelines;
using Core.CrossCuttingConcerns.Exceptions.Types;
using Domain.Entities;

namespace Application.Features.CreditSubTypes.Rules;

public class CreditSubTypeBusinessRules : BaseBusinessRules
{
    private readonly ICreditSubTypeRepository _creditSubTypeRepository;
    private readonly ICreditTypeRepository _creditTypeRepository;

    public CreditSubTypeBusinessRules(
        ICreditSubTypeRepository creditSubTypeRepository,
        ICreditTypeRepository creditTypeRepository)
    {
        _creditSubTypeRepository = creditSubTypeRepository;
        _creditTypeRepository = creditTypeRepository;
    }

    public Task CreditSubTypeShouldExistWhenSelected(CreditSubType? creditSubType)
    {
        if (creditSubType is null)
            throw new BusinessException(CreditSubTypeMessages.NotExists);

        return Task.CompletedTask;
    }

    public async Task CreditTypeShouldExist(Guid creditTypeId)
    {
        bool exists = await _creditTypeRepository.AnyAsync(c => c.Id == creditTypeId);
        if (!exists)
            throw new BusinessException(CreditSubTypeMessages.CreditTypeNotExists);
    }

    public async Task CodeShouldBeUnique(string code, Guid? excludeId = null)
    {
        bool exists = await _creditSubTypeRepository.AnyAsync(
            c => c.Code == code && (!excludeId.HasValue || c.Id != excludeId.Value));

        if (exists)
            throw new BusinessException(CreditSubTypeMessages.CodeAlreadyExists);
    }

    public Task AmountRangeShouldBeValid(decimal minAmount, decimal maxAmount)
    {
        if (minAmount > maxAmount)
            throw new BusinessException(CreditSubTypeMessages.InvalidAmountRange);

        return Task.CompletedTask;
    }

    public Task TermRangeShouldBeValid(int minTermMonths, int maxTermMonths)
    {
        if (minTermMonths > maxTermMonths)
            throw new BusinessException(CreditSubTypeMessages.InvalidTermRange);

        return Task.CompletedTask;
    }
}
