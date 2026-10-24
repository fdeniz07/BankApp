using Application.Modules.Credit.Features.CreditTypes.Rules;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;

namespace Application.Modules.Credit.Features.CreditTypes.Commands.Create;

public class CreateCreditTypeRequestHandler : IRequestHandler<CreateCreditTypeRequest, CreatedCreditTypeResponse>
{
    private readonly ICreditTypeRepository _creditTypeRepository;
    private readonly IMapper _mapper;
    private readonly CreditTypeBusinessRules _businessRules;

    public CreateCreditTypeRequestHandler(
        ICreditTypeRepository creditTypeRepository,
        IMapper mapper,
        CreditTypeBusinessRules businessRules)
    {
        _creditTypeRepository = creditTypeRepository;
        _mapper = mapper;
        _businessRules = businessRules;
    }

    public async Task<CreatedCreditTypeResponse> Handle(
        CreateCreditTypeRequest request,
        CancellationToken cancellationToken)
    {
        await _businessRules.CodeShouldBeUnique(request.Code);

        CreditType creditType = _mapper.Map<CreditType>(request);
        await _creditTypeRepository.AddAsync(creditType, cancellationToken);

        return _mapper.Map<CreatedCreditTypeResponse>(creditType);
    }
}
