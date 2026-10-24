using Core.Application.Pipelines.Authorization;
using Core.Security.Constants;
using MediatR;

namespace Application.Modules.Customers.Features.CorporateCustomers.Queries.GetCatalog;

public class GetCorporateCustomerCatalogRequest
    : IRequest<GetCorporateCustomerCatalogResponse>,
      ISecuredRequest
{
    public string[] Roles =>
        [GeneralOperationClaims.BankStaff, GeneralOperationClaims.BankManager];
}
