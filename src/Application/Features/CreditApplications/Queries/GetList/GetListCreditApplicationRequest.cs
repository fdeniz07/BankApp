using Core.Application.Requests;
using Core.Application.Responses;
using Domain.Enums;
using MediatR;

namespace Application.Features.CreditApplications.Queries.GetList;

public class GetListCreditApplicationRequest : IRequest<GetListResponse<GetListCreditApplicationListItemDto>>
{
    public PageRequest PageRequest { get; set; } = new();
    public Guid? CustomerId { get; set; }
    public CreditApplicationStatus? Status { get; set; }
}
