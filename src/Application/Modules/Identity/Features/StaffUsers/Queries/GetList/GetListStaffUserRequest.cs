using Core.Application.Pipelines.Authorization;
using Core.Application.Requests;
using Core.Application.Responses;
using Core.Security.Constants;
using MediatR;

namespace Application.Modules.Identity.Features.StaffUsers.Queries.GetList;

public class GetListStaffUserRequest
    : IRequest<GetListResponse<GetListStaffUserListItemDto>>,
        ISecuredRequest
{
    public string[] Roles => [GeneralOperationClaims.BankManager, GeneralOperationClaims.SuperAdmin];
    public PageRequest PageRequest { get; set; } = new();
    public bool? IsActive { get; set; }
    public Guid? BranchId { get; set; }
}
