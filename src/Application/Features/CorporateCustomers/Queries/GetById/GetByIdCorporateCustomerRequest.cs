using Core.Application.Pipelines.Authorization;
using Core.Security.Constants;
using MediatR;

namespace Application.Features.CorporateCustomers.Queries.GetById;

public class GetByIdCorporateCustomerRequest : IRequest<GetByIdCorporateCustomerResponse>, ISecuredRequest
{
    public string[] Roles => [GeneralOperationClaims.Customer, GeneralOperationClaims.BankStaff];
    public Guid Id { get; set; }
}
