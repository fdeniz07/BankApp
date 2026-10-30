using Core.Application.Pipelines.Authorization;
using Core.Security.Constants;
using MediatR;

namespace Application.Modules.Identity.Features.DecisionLetterTemplates.Commands.Delete;

public class DeleteDecisionLetterTemplateRequest : IRequest<DeletedDecisionLetterTemplateResponse>, ISecuredRequest
{
    public string[] Roles =>
    [
        GeneralOperationClaims.BankStaff,
        GeneralOperationClaims.BankManager,
        GeneralOperationClaims.SuperAdmin
    ];

    public Guid Id { get; set; }
}
