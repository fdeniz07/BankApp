using Core.Application.Pipelines.Authorization;
using Core.Security.Constants;
using MediatR;

namespace Application.Modules.Identity.Features.DecisionLetterTemplates.Commands.Create;

public class CreateDecisionLetterTemplateRequest
    : IRequest<CreatedDecisionLetterTemplateResponse>,
      ISecuredRequest
{
    public string[] Roles =>
    [
        GeneralOperationClaims.BankStaff,
        GeneralOperationClaims.BankManager,
        GeneralOperationClaims.SuperAdmin
    ];

    public string Kind { get; set; } = "Approve";
    public string Locale { get; set; } = "tr";
    public string Name { get; set; } = default!;
    public string Body { get; set; } = default!;
}
