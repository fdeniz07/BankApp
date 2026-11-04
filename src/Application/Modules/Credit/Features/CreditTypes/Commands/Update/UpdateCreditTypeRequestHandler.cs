using Application.Modules.Credit.Features.CreditTypes.Rules;
using Application.Services.Repositories;
using Application.Services.Search;
using AutoMapper;
using Domain.Entities;
using MediatR;

namespace Application.Modules.Credit.Features.CreditTypes.Commands.Update;

public class UpdateCreditTypeRequestHandler : IRequestHandler<UpdateCreditTypeRequest, UpdatedCreditTypeResponse>
{
    private readonly ICreditTypeRepository _creditTypeRepository;
    private readonly IMapper _mapper;
    private readonly CreditTypeBusinessRules _businessRules;
    private readonly ICreditTypeSearchService _search;

    public UpdateCreditTypeRequestHandler(
        ICreditTypeRepository creditTypeRepository,
        IMapper mapper,
        CreditTypeBusinessRules businessRules,
        ICreditTypeSearchService search)
    {
        _creditTypeRepository = creditTypeRepository;
        _mapper = mapper;
        _businessRules = businessRules;
        _search = search;
    }

    public async Task<UpdatedCreditTypeResponse> Handle(
        UpdateCreditTypeRequest request,
        CancellationToken cancellationToken)
    {
        CreditType? creditType = await _creditTypeRepository.GetAsync(
            predicate: c => c.Id == request.Id,
            cancellationToken: cancellationToken);

        await _businessRules.CreditTypeShouldExistWhenSelected(creditType);
        await _businessRules.CodeShouldBeUnique(request.Code, request.Id);

        creditType = _mapper.Map(request, creditType);
        await _creditTypeRepository.UpdateAsync(creditType!, cancellationToken);
        await _search.UpsertFromCreditTypeAsync(creditType!, cancellationToken);

        return _mapper.Map<UpdatedCreditTypeResponse>(creditType);
    }
}
