using Application.Modules.Credit.Features.CreditTypes.Rules;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;

namespace Application.Modules.Credit.Features.CreditTypes.Commands.Delete;

public class DeleteCreditTypeRequestHandler : IRequestHandler<DeleteCreditTypeRequest, DeletedCreditTypeResponse>
{
    private readonly ICreditTypeRepository _creditTypeRepository;
    private readonly IMapper _mapper;
    private readonly CreditTypeBusinessRules _businessRules;

    public DeleteCreditTypeRequestHandler(
        ICreditTypeRepository creditTypeRepository,
        IMapper mapper,
        CreditTypeBusinessRules businessRules)
    {
        _creditTypeRepository = creditTypeRepository;
        _mapper = mapper;
        _businessRules = businessRules;
    }

    public async Task<DeletedCreditTypeResponse> Handle(
        DeleteCreditTypeRequest request,
        CancellationToken cancellationToken)
    {
        CreditType? creditType = await _creditTypeRepository.GetAsync(
            predicate: c => c.Id == request.Id,
            cancellationToken: cancellationToken);

        await _businessRules.CreditTypeShouldExistWhenSelected(creditType);

        creditType!.IsActive = false;
        await _creditTypeRepository.DeleteAsync(creditType, cancellationToken: cancellationToken);

        return _mapper.Map<DeletedCreditTypeResponse>(creditType);
    }
}
