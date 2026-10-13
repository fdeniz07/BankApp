using Application.Services.Repositories;
using AutoMapper;
using Core.Application.Responses;
using Core.Repositories;
using Domain.Entities;
using MediatR;

namespace Application.Features.CorporateCustomers.Queries.GetList;

public class GetListCorporateCustomerRequestHandler
    : IRequestHandler<GetListCorporateCustomerRequest, GetListResponse<GetListCorporateCustomerListItemDto>>
{
    private readonly ICorporateCustomerRepository _corporateCustomerRepository;
    private readonly IMapper _mapper;

    public GetListCorporateCustomerRequestHandler(
        ICorporateCustomerRepository corporateCustomerRepository,
        IMapper mapper)
    {
        _corporateCustomerRepository = corporateCustomerRepository;
        _mapper = mapper;
    }

    public async Task<GetListResponse<GetListCorporateCustomerListItemDto>> Handle(
        GetListCorporateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        IPaginate<CorporateCustomer> corporateCustomers = await _corporateCustomerRepository.GetListAsync(
            index: request.PageRequest.PageIndex,
            size: request.PageRequest.PageSize,
            cancellationToken: cancellationToken);

        return _mapper.Map<GetListResponse<GetListCorporateCustomerListItemDto>>(corporateCustomers);
    }
}
