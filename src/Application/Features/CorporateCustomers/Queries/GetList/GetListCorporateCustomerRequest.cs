using Core.Application.Requests;
using Core.Application.Responses;
using MediatR;

namespace Application.Features.CorporateCustomers.Queries.GetList;

public class GetListCorporateCustomerRequest : IRequest<GetListResponse<GetListCorporateCustomerListItemDto>>
{
    public PageRequest PageRequest { get; set; } = new();
}
