using Core.Repositories;

namespace Core.Security.Entities;

public class UserOperationClaim : Entity
{
    public Guid UserId { get; set; }
    public Guid OperationClaimId { get; set; }

    public User User { get; set; } = default!;
    public OperationClaim OperationClaim { get; set; } = default!;
}
