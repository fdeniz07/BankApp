using Application.Modules.Credit.Features.CreditTypes.Constants;
using Application.Services.Repositories;
using Core.Application.Pipelines;
using Core.CrossCuttingConcerns.Exceptions.Types;
using Domain.Entities;

namespace Application.Modules.Credit.Features.CreditTypes.Rules;

public class CreditTypeBusinessRules : BaseBusinessRules
{
    private readonly ICreditTypeRepository _creditTypeRepository;

    public CreditTypeBusinessRules(ICreditTypeRepository creditTypeRepository)
    {
        _creditTypeRepository = creditTypeRepository;
    }

    public Task CreditTypeShouldExistWhenSelected(CreditType? creditType)
    {
        if (creditType is null)
            throw new BusinessException(CreditTypeMessages.NotExists);

        return Task.CompletedTask;
    }

    public async Task CodeShouldBeUnique(string code, Guid? excludeId = null)
    {
        bool exists = await _creditTypeRepository.AnyAsync(
            c => c.Code == code && (!excludeId.HasValue || c.Id != excludeId.Value));

        if (exists)
            throw new BusinessException(CreditTypeMessages.CodeAlreadyExists);
    }
}
