using Core.Application.Requests;
using Core.Application.Responses;
using MediatR;

namespace Application.Features.CreditSubTypes.Queries.GetList;

public class GetListCreditSubTypeRequest : IRequest<GetListResponse<GetListCreditSubTypeListItemDto>>
{
    public PageRequest PageRequest { get; set; } = new();
    public Guid? CreditTypeId { get; set; }
}
