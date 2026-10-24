using Application.Modules.Credit.Features.CreditTypes.Rules;
using Application.Services.Repositories;
using Domain.Entities;
using MediatR;

namespace Application.Modules.Credit.Features.CreditTypes.Commands.SetActive;

public class SetCreditTypeActiveRequestHandler
    : IRequestHandler<SetCreditTypeActiveRequest, SetCreditTypeActiveResponse>
{
    private readonly ICreditTypeRepository _creditTypeRepository;
    private readonly CreditTypeBusinessRules _businessRules;

    public SetCreditTypeActiveRequestHandler(
        ICreditTypeRepository creditTypeRepository,
        CreditTypeBusinessRules businessRules)
    {
        _creditTypeRepository = creditTypeRepository;
        _businessRules = businessRules;
    }

    public async Task<SetCreditTypeActiveResponse> Handle(
        SetCreditTypeActiveRequest request,
        CancellationToken cancellationToken)
    {
        CreditType? creditType = await _creditTypeRepository.GetAsync(
            predicate: c => c.Id == request.Id,
            cancellationToken: cancellationToken);

        await _businessRules.CreditTypeShouldExistWhenSelected(creditType);

        creditType!.IsActive = request.IsActive;
        await _creditTypeRepository.UpdateAsync(creditType, cancellationToken);

        return new SetCreditTypeActiveResponse
        {
            Id = creditType.Id,
            Name = creditType.Name,
            Code = creditType.Code,
            IsActive = creditType.IsActive
        };
    }
}
