using Core.Application.Pipelines.Authorization;
using Core.Security.Constants;
using Domain.Enums;
using MediatR;

namespace Application.Modules.Identity.Features.StaffUsers.Commands.Update;

public class UpdateStaffUserRequest : IRequest<UpdatedStaffUserResponse>, ISecuredRequest
{
    public string[] Roles => [GeneralOperationClaims.BankStaff, GeneralOperationClaims.BankManager];
    public Guid Id { get; set; }
    public string Email { get; set; } = default!;
    public string? NewPassword { get; set; }
    public bool IsActive { get; set; }
    public bool IsManager { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public PersonTitle? Title { get; set; }
    public Gender? Gender { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public StaffPosition? Position { get; set; }
}
