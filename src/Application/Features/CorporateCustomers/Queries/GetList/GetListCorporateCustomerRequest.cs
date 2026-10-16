using Core.Application.Requests;
using Core.Application.Responses;
using Core.Application.Pipelines.Authorization;
using Core.Security.Constants;
using MediatR;

namespace Application.Features.CorporateCustomers.Queries.GetList;

public class GetListCorporateCustomerRequest : IRequest<GetListResponse<GetListCorporateCustomerListItemDto>>, ISecuredRequest
{
    public string[] Roles =>
        [GeneralOperationClaims.Customer, GeneralOperationClaims.BankStaff, GeneralOperationClaims.BankManager];
    public PageRequest PageRequest { get; set; } = new();
    public bool? IsActive { get; set; }
    public bool OnlyDeleted { get; set; }
}
