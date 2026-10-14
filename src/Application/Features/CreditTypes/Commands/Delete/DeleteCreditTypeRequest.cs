using Core.Application.Pipelines.Authorization;
using Core.Security.Constants;
using MediatR;

namespace Application.Features.CreditTypes.Commands.Delete;

public class DeleteCreditTypeRequest : IRequest<DeletedCreditTypeResponse>, ISecuredRequest
{
    public string[] Roles => [GeneralOperationClaims.BankStaff];
    public Guid Id { get; set; }
}
