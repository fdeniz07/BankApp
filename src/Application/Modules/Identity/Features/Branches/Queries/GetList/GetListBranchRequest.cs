using Core.Application.Pipelines.Authorization;
using Core.Security.Constants;
using MediatR;

namespace Application.Modules.Identity.Features.Branches.Queries.GetList;

public class GetListBranchRequest : IRequest<IReadOnlyList<GetListBranchListItemDto>>, ISecuredRequest
{
    public string[] Roles =>
    [
        GeneralOperationClaims.BankStaff,
        GeneralOperationClaims.BankManager,
        GeneralOperationClaims.SuperAdmin
    ];

    public bool? IsActive { get; set; } = true;
}
