using Application.Modules.Credit.Features.CreditSubTypes.Rules;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;

namespace Application.Modules.Credit.Features.CreditSubTypes.Commands.Delete;

public class DeleteCreditSubTypeRequestHandler
    : IRequestHandler<DeleteCreditSubTypeRequest, DeletedCreditSubTypeResponse>
{
    private readonly ICreditSubTypeRepository _creditSubTypeRepository;
    private readonly IMapper _mapper;
    private readonly CreditSubTypeBusinessRules _businessRules;

    public DeleteCreditSubTypeRequestHandler(
        ICreditSubTypeRepository creditSubTypeRepository,
        IMapper mapper,
        CreditSubTypeBusinessRules businessRules)
    {
        _creditSubTypeRepository = creditSubTypeRepository;
        _mapper = mapper;
        _businessRules = businessRules;
    }

    public async Task<DeletedCreditSubTypeResponse> Handle(
        DeleteCreditSubTypeRequest request,
        CancellationToken cancellationToken)
    {
        CreditSubType? creditSubType = await _creditSubTypeRepository.GetAsync(
            predicate: c => c.Id == request.Id,
            cancellationToken: cancellationToken);

        await _businessRules.CreditSubTypeShouldExistWhenSelected(creditSubType);

        creditSubType!.IsActive = false;
        await _creditSubTypeRepository.DeleteAsync(creditSubType, cancellationToken: cancellationToken);

        return _mapper.Map<DeletedCreditSubTypeResponse>(creditSubType);
    }
}
