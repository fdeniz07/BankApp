using Application.Modules.Credit.Features.CreditTypes.Rules;
using Application.Services.Repositories;
using Application.Services.Search;
using AutoMapper;
using Domain.Entities;
using MediatR;

namespace Application.Modules.Credit.Features.CreditTypes.Commands.Create;

public class CreateCreditTypeRequestHandler : IRequestHandler<CreateCreditTypeRequest, CreatedCreditTypeResponse>
{
    private readonly ICreditTypeRepository _creditTypeRepository;
    private readonly IMapper _mapper;
    private readonly CreditTypeBusinessRules _businessRules;
    private readonly ICreditTypeSearchService _search;

    public CreateCreditTypeRequestHandler(
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

    public async Task<CreatedCreditTypeResponse> Handle(
        CreateCreditTypeRequest request,
        CancellationToken cancellationToken)
    {
        await _businessRules.CodeShouldBeUnique(request.Code);

        CreditType creditType = _mapper.Map<CreditType>(request);
        await _creditTypeRepository.AddAsync(creditType, cancellationToken);
        await _search.UpsertFromCreditTypeAsync(creditType, cancellationToken);

        return _mapper.Map<CreatedCreditTypeResponse>(creditType);
    }
}
