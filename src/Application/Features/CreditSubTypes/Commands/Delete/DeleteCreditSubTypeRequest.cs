using Core.Application.Pipelines.Authorization;
using Core.Security.Constants;
using MediatR;

namespace Application.Features.CreditSubTypes.Commands.Delete;

public class DeleteCreditSubTypeRequest : IRequest<DeletedCreditSubTypeResponse>, ISecuredRequest
{
    public string[] Roles => [GeneralOperationClaims.BankStaff];
    public Guid Id { get; set; }
}
