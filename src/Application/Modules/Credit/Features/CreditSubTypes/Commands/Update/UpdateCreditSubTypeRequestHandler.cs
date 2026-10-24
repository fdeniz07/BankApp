using Application.Modules.Credit.Features.CreditSubTypes.Rules;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;

namespace Application.Modules.Credit.Features.CreditSubTypes.Commands.Update;

public class UpdateCreditSubTypeRequestHandler
    : IRequestHandler<UpdateCreditSubTypeRequest, UpdatedCreditSubTypeResponse>
{
    private readonly ICreditSubTypeRepository _creditSubTypeRepository;
    private readonly IMapper _mapper;
    private readonly CreditSubTypeBusinessRules _businessRules;

    public UpdateCreditSubTypeRequestHandler(
        ICreditSubTypeRepository creditSubTypeRepository,
        IMapper mapper,
        CreditSubTypeBusinessRules businessRules)
    {
        _creditSubTypeRepository = creditSubTypeRepository;
        _mapper = mapper;
        _businessRules = businessRules;
    }

    public async Task<UpdatedCreditSubTypeResponse> Handle(
        UpdateCreditSubTypeRequest request,
        CancellationToken cancellationToken)
    {
        CreditSubType? creditSubType = await _creditSubTypeRepository.GetAsync(
            predicate: c => c.Id == request.Id,
            cancellationToken: cancellationToken);

        await _businessRules.CreditSubTypeShouldExistWhenSelected(creditSubType);
        await _businessRules.CreditTypeShouldExist(request.CreditTypeId);
        await _businessRules.CodeShouldBeUnique(request.Code, request.Id);
        await _businessRules.AmountRangeShouldBeValid(request.MinAmount, request.MaxAmount);
        await _businessRules.TermRangeShouldBeValid(request.MinTermMonths, request.MaxTermMonths);

        creditSubType = _mapper.Map(request, creditSubType);
        await _creditSubTypeRepository.UpdateAsync(creditSubType!, cancellationToken);

        return _mapper.Map<UpdatedCreditSubTypeResponse>(creditSubType);
    }
}
