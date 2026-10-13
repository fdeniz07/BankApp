using Application.Features.CreditSubTypes.Rules;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;

namespace Application.Features.CreditSubTypes.Commands.Create;

public class CreateCreditSubTypeRequestHandler
    : IRequestHandler<CreateCreditSubTypeRequest, CreatedCreditSubTypeResponse>
{
    private readonly ICreditSubTypeRepository _creditSubTypeRepository;
    private readonly IMapper _mapper;
    private readonly CreditSubTypeBusinessRules _businessRules;

    public CreateCreditSubTypeRequestHandler(
        ICreditSubTypeRepository creditSubTypeRepository,
        IMapper mapper,
        CreditSubTypeBusinessRules businessRules)
    {
        _creditSubTypeRepository = creditSubTypeRepository;
        _mapper = mapper;
        _businessRules = businessRules;
    }

    public async Task<CreatedCreditSubTypeResponse> Handle(
        CreateCreditSubTypeRequest request,
        CancellationToken cancellationToken)
    {
        await _businessRules.CreditTypeShouldExist(request.CreditTypeId);
        await _businessRules.CodeShouldBeUnique(request.Code);
        await _businessRules.AmountRangeShouldBeValid(request.MinAmount, request.MaxAmount);
        await _businessRules.TermRangeShouldBeValid(request.MinTermMonths, request.MaxTermMonths);

        CreditSubType creditSubType = _mapper.Map<CreditSubType>(request);
        await _creditSubTypeRepository.AddAsync(creditSubType, cancellationToken);

        return _mapper.Map<CreatedCreditSubTypeResponse>(creditSubType);
    }
}
