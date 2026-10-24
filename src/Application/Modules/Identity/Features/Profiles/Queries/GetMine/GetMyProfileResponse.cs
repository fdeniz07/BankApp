using Domain.Enums;

namespace Application.Modules.Identity.Features.Profiles.Queries.GetMine;

public class GetMyProfileResponse
{
    public Guid Id { get; set; }
    public string Email { get; set; } = default!;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public PersonTitle? Title { get; set; }
    public Gender? Gender { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public StaffPosition? Position { get; set; }
    public string? ProfileImageUrl { get; set; }
    public bool IsActive { get; set; }
    public bool IsStaff { get; set; }
    public IList<string> Roles { get; set; } = [];
}
