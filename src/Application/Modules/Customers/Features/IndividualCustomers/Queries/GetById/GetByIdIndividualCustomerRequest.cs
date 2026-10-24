using Core.Application.Pipelines.Authorization;
using Core.Security.Constants;
using MediatR;

namespace Application.Modules.Customers.Features.IndividualCustomers.Queries.GetById;

public class GetByIdIndividualCustomerRequest : IRequest<GetByIdIndividualCustomerResponse>, ISecuredRequest
{
    public string[] Roles =>
        [GeneralOperationClaims.Customer, GeneralOperationClaims.BankStaff, GeneralOperationClaims.BankManager];
    public Guid Id { get; set; }
}
