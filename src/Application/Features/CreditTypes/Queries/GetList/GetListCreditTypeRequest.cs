using Core.Application.Requests;
using Core.Application.Responses;
using Core.Application.Pipelines.Authorization;
using Core.Security.Constants;
using MediatR;

namespace Application.Features.CreditTypes.Queries.GetList;

public class GetListCreditTypeRequest : IRequest<GetListResponse<GetListCreditTypeListItemDto>>, ISecuredRequest
{
    public string[] Roles => [GeneralOperationClaims.Customer, GeneralOperationClaims.BankStaff];
    public PageRequest PageRequest { get; set; } = new();
}
