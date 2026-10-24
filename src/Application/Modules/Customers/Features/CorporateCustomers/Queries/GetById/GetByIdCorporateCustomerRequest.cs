using Core.Application.Pipelines.Authorization;
using Core.Security.Constants;
using MediatR;

namespace Application.Modules.Customers.Features.CorporateCustomers.Queries.GetById;

public class GetByIdCorporateCustomerRequest : IRequest<GetByIdCorporateCustomerResponse>, ISecuredRequest
{
    public string[] Roles =>
        [GeneralOperationClaims.Customer, GeneralOperationClaims.BankStaff, GeneralOperationClaims.BankManager];
    public Guid Id { get; set; }
}
