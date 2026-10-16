using Core.Application.Pipelines.Authorization;
using Core.Security.Constants;
using MediatR;

namespace Application.Features.IndividualCustomers.Commands.Delete;

public class DeleteIndividualCustomerRequest : IRequest<DeletedIndividualCustomerResponse>, ISecuredRequest
{
    public string[] Roles => [GeneralOperationClaims.BankManager];
    public Guid Id { get; set; }
}
