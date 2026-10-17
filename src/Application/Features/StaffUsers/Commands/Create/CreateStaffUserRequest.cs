using Core.Application.Pipelines.Authorization;
using Core.Security.Constants;
using Domain.Enums;
using MediatR;

namespace Application.Features.StaffUsers.Commands.Create;

public class CreateStaffUserRequest : IRequest<CreatedStaffUserResponse>, ISecuredRequest
{
    public string[] Roles => [GeneralOperationClaims.BankManager];
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
    public bool IsManager { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public PersonTitle? Title { get; set; }
    public Gender? Gender { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public StaffPosition? Position { get; set; }
}
