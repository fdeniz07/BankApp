namespace Application.Services.Search;

public sealed class StaffUserSearchDocument
{
    public Guid Id { get; set; }
    public string Email { get; set; } = "";
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Title { get; set; }
    public string? Gender { get; set; }
    public string? DateOfBirth { get; set; }
    public string? Position { get; set; }
    public string? BranchId { get; set; }
    public string? BranchCode { get; set; }
    public string? BranchName { get; set; }
    public bool IsActive { get; set; }
    public bool IsManager { get; set; }
    public List<string> Roles { get; set; } = [];
    public string? ProfileImageUrl { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
}
