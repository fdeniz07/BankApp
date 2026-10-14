using Core.Repositories;

namespace Core.Security.Entities;

public class User : Entity
{
    public string Email { get; set; } = default!;
    public byte[] PasswordSalt { get; set; } = default!;
    public byte[] PasswordHash { get; set; } = default!;
    public Guid? CustomerId { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<UserOperationClaim> UserOperationClaims { get; set; } = [];
}
