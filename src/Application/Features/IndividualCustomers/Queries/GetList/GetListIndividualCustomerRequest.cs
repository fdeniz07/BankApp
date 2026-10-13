using Core.Application.Requests;
using Core.Application.Responses;
using MediatR;

namespace Application.Features.IndividualCustomers.Queries.GetList;

public class GetListIndividualCustomerRequest : IRequest<GetListResponse<GetListIndividualCustomerListItemDto>>
{
    public PageRequest PageRequest { get; set; } = new();
    public bool? IsActive { get; set; }
    public bool OnlyDeleted { get; set; }
}
