using Core.Application.Requests;
using Core.Application.Responses;
using Core.Application.Pipelines.Authorization;
using Core.Security.Constants;
using MediatR;

namespace Application.Features.IndividualCustomers.Queries.GetList;

public class GetListIndividualCustomerRequest : IRequest<GetListResponse<GetListIndividualCustomerListItemDto>>, ISecuredRequest
{
    public string[] Roles => [GeneralOperationClaims.Customer, GeneralOperationClaims.BankStaff];
    public PageRequest PageRequest { get; set; } = new();
    public bool? IsActive { get; set; }
    public bool OnlyDeleted { get; set; }
}
