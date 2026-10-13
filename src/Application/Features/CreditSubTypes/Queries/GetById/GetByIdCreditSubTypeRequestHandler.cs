using Application.Features.CreditSubTypes.Rules;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;

namespace Application.Features.CreditSubTypes.Queries.GetById;

public class GetByIdCreditSubTypeRequestHandler
    : IRequestHandler<GetByIdCreditSubTypeRequest, GetByIdCreditSubTypeResponse>
{
    private readonly ICreditSubTypeRepository _creditSubTypeRepository;
    private readonly IMapper _mapper;
    private readonly CreditSubTypeBusinessRules _businessRules;

    public GetByIdCreditSubTypeRequestHandler(
        ICreditSubTypeRepository creditSubTypeRepository,
        IMapper mapper,
        CreditSubTypeBusinessRules businessRules)
    {
        _creditSubTypeRepository = creditSubTypeRepository;
        _mapper = mapper;
        _businessRules = businessRules;
    }

    public async Task<GetByIdCreditSubTypeResponse> Handle(
        GetByIdCreditSubTypeRequest request,
        CancellationToken cancellationToken)
    {
        CreditSubType? creditSubType = await _creditSubTypeRepository.GetAsync(
            predicate: c => c.Id == request.Id,
            cancellationToken: cancellationToken);

        await _businessRules.CreditSubTypeShouldExistWhenSelected(creditSubType);

        return _mapper.Map<GetByIdCreditSubTypeResponse>(creditSubType);
    }
}
