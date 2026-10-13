using System.Linq.Expressions;
using Application.Services.Repositories;
using AutoMapper;
using Core.Application.Responses;
using Core.Repositories;
using Domain.Entities;
using MediatR;

namespace Application.Features.CreditSubTypes.Queries.GetList;

public class GetListCreditSubTypeRequestHandler
    : IRequestHandler<GetListCreditSubTypeRequest, GetListResponse<GetListCreditSubTypeListItemDto>>
{
    private readonly ICreditSubTypeRepository _creditSubTypeRepository;
    private readonly IMapper _mapper;

    public GetListCreditSubTypeRequestHandler(ICreditSubTypeRepository creditSubTypeRepository, IMapper mapper)
    {
        _creditSubTypeRepository = creditSubTypeRepository;
        _mapper = mapper;
    }

    public async Task<GetListResponse<GetListCreditSubTypeListItemDto>> Handle(
        GetListCreditSubTypeRequest request,
        CancellationToken cancellationToken)
    {
        Expression<Func<CreditSubType, bool>>? predicate = request.CreditTypeId.HasValue
            ? c => c.CreditTypeId == request.CreditTypeId.Value
            : null;

        IPaginate<CreditSubType> creditSubTypes = await _creditSubTypeRepository.GetListAsync(
            predicate: predicate,
            index: request.PageRequest.PageIndex,
            size: request.PageRequest.PageSize,
            cancellationToken: cancellationToken);

        return _mapper.Map<GetListResponse<GetListCreditSubTypeListItemDto>>(creditSubTypes);
    }
}
