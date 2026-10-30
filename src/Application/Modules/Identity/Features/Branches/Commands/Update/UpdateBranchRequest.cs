using Core.Application.Pipelines.Authorization;
using Core.Security.Constants;
using MediatR;

namespace Application.Modules.Identity.Features.Branches.Commands.Update;

public class UpdateBranchRequest : IRequest<UpdatedBranchResponse>, ISecuredRequest
{
    public string[] Roles => [GeneralOperationClaims.SuperAdmin];
    public Guid Id { get; set; }
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? City { get; set; }
    public bool IsActive { get; set; } = true;
}
