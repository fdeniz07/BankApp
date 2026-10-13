using Application.Services.Repositories;
using AutoMapper;
using Core.Application.Responses;
using Core.Repositories;
using Domain.Entities;
using MediatR;

namespace Application.Features.CreditTypes.Queries.GetList;

public class GetListCreditTypeRequestHandler
    : IRequestHandler<GetListCreditTypeRequest, GetListResponse<GetListCreditTypeListItemDto>>
{
    private readonly ICreditTypeRepository _creditTypeRepository;
    private readonly IMapper _mapper;

    public GetListCreditTypeRequestHandler(ICreditTypeRepository creditTypeRepository, IMapper mapper)
    {
        _creditTypeRepository = creditTypeRepository;
        _mapper = mapper;
    }

    public async Task<GetListResponse<GetListCreditTypeListItemDto>> Handle(
        GetListCreditTypeRequest request,
        CancellationToken cancellationToken)
    {
        IPaginate<CreditType> creditTypes = await _creditTypeRepository.GetListAsync(
            index: request.PageRequest.PageIndex,
            size: request.PageRequest.PageSize,
            cancellationToken: cancellationToken);

        return _mapper.Map<GetListResponse<GetListCreditTypeListItemDto>>(creditTypes);
    }
}
