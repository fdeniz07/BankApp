using Core.Application.Pipelines.Authorization;
using Core.Security.Constants;
using MediatR;

namespace Application.Modules.Identity.Features.DecisionLetterTemplates.Queries.GetList;

public class GetListDecisionLetterTemplateRequest
    : IRequest<IReadOnlyList<GetListDecisionLetterTemplateItemDto>>,
      ISecuredRequest
{
    public string[] Roles =>
    [
        GeneralOperationClaims.BankStaff,
        GeneralOperationClaims.BankManager,
        GeneralOperationClaims.SuperAdmin
    ];

    public string? Kind { get; set; }
}
