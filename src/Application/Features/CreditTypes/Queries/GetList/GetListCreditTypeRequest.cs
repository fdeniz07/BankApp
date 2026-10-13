using Core.Application.Requests;
using Core.Application.Responses;
using MediatR;

namespace Application.Features.CreditTypes.Queries.GetList;

public class GetListCreditTypeRequest : IRequest<GetListResponse<GetListCreditTypeListItemDto>>
{
    public PageRequest PageRequest { get; set; } = new();
}
