using Core.Repositories;

namespace Core.Security.Entities;

public class OperationClaim : Entity
{
    public string Name { get; set; } = default!;

    public ICollection<UserOperationClaim> UserOperationClaims { get; set; } = [];
}
