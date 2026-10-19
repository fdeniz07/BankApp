using Core.Security.Entities;
using Domain.Enums;

namespace Domain.Entities;

public class ApplicationUser : User
{
    public Guid? CustomerId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public PersonTitle? Title { get; set; }
    public Gender? Gender { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public StaffPosition? Position { get; set; }
    public string? ProfileImagePath { get; set; }
    public bool EmailConfirmed { get; set; }
    public string? EmailConfirmationToken { get; set; }
    public DateTimeOffset? EmailConfirmationTokenExpiresAt { get; set; }

    public Customer? Customer { get; set; }
}
