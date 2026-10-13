using Application.Services.Repositories;
using AutoMapper;
using Core.Application.Responses;
using Core.Repositories;
using Domain.Entities;
using MediatR;

namespace Application.Features.IndividualCustomers.Queries.GetList;

public class GetListIndividualCustomerRequestHandler
    : IRequestHandler<GetListIndividualCustomerRequest, GetListResponse<GetListIndividualCustomerListItemDto>>
{
    private readonly IIndividualCustomerRepository _individualCustomerRepository;
    private readonly IMapper _mapper;

    public GetListIndividualCustomerRequestHandler(
        IIndividualCustomerRepository individualCustomerRepository,
        IMapper mapper)
    {
        _individualCustomerRepository = individualCustomerRepository;
        _mapper = mapper;
    }

    public async Task<GetListResponse<GetListIndividualCustomerListItemDto>> Handle(
        GetListIndividualCustomerRequest request,
        CancellationToken cancellationToken)
    {
        IPaginate<IndividualCustomer> individualCustomers = await _individualCustomerRepository.GetListAsync(
            index: request.PageRequest.PageIndex,
            size: request.PageRequest.PageSize,
            cancellationToken: cancellationToken);

        return _mapper.Map<GetListResponse<GetListIndividualCustomerListItemDto>>(individualCustomers);
    }
}
