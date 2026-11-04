using Application.Modules.Credit.Features.CreditTypes.Rules;
using Application.Services.Repositories;
using Application.Services.Search;
using Domain.Entities;
using MediatR;

namespace Application.Modules.Credit.Features.CreditTypes.Commands.SetActive;

public class SetCreditTypeActiveRequestHandler
    : IRequestHandler<SetCreditTypeActiveRequest, SetCreditTypeActiveResponse>
{
    private readonly ICreditTypeRepository _creditTypeRepository;
    private readonly CreditTypeBusinessRules _businessRules;
    private readonly ICreditTypeSearchService _search;

    public SetCreditTypeActiveRequestHandler(
        ICreditTypeRepository creditTypeRepository,
        CreditTypeBusinessRules businessRules,
        ICreditTypeSearchService search)
    {
        _creditTypeRepository = creditTypeRepository;
        _businessRules = businessRules;
        _search = search;
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
        await _search.UpsertFromCreditTypeAsync(creditType, cancellationToken);

        return new SetCreditTypeActiveResponse
        {
            Id = creditType.Id,
            Name = creditType.Name,
            Code = creditType.Code,
            IsActive = creditType.IsActive
        };
    }
}
