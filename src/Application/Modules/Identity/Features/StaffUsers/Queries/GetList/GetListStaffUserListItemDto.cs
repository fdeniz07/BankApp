using Domain.Enums;

namespace Application.Modules.Identity.Features.StaffUsers.Queries.GetList;

public class GetListStaffUserListItemDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = default!;
    public bool IsActive { get; set; }
    public bool IsManager { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public PersonTitle? Title { get; set; }
    public Gender? Gender { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public StaffPosition? Position { get; set; }
    public string? ProfileImageUrl { get; set; }
    public IList<string> Roles { get; set; } = [];
    public DateTimeOffset CreatedDate { get; set; }
}
