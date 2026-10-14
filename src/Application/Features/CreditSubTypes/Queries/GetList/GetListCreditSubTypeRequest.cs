using Core.Application.Requests;
using Core.Application.Responses;
using Core.Application.Pipelines.Authorization;
using Core.Security.Constants;
using MediatR;

namespace Application.Features.CreditSubTypes.Queries.GetList;

public class GetListCreditSubTypeRequest : IRequest<GetListResponse<GetListCreditSubTypeListItemDto>>, ISecuredRequest
{
    public string[] Roles => [GeneralOperationClaims.Customer, GeneralOperationClaims.BankStaff];
    public PageRequest PageRequest { get; set; } = new();
    public Guid? CreditTypeId { get; set; }
}
