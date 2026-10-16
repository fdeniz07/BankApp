using Core.Application.Pipelines.Authorization;
using Core.Security.Constants;
using MediatR;

namespace Application.Features.CorporateCustomers.Commands.Delete;

public class DeleteCorporateCustomerRequest : IRequest<DeletedCorporateCustomerResponse>, ISecuredRequest
{
    public string[] Roles => [GeneralOperationClaims.BankManager];
    public Guid Id { get; set; }
}
