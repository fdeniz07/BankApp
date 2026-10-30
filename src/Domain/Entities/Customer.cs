using Core.Repositories;

namespace Domain.Entities;

public abstract class Customer : Entity
{
    public string CustomerNumber { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string PhoneNumber { get; set; } = default!;
    public string Address { get; set; } = default!;
    public bool IsActive { get; set; } = true;
    /// <summary>Home branch for RBAC (staff see own branch; managers read others).</summary>
    public Guid? BranchId { get; set; }

    public Branch? Branch { get; set; }
    public ICollection<CreditApplication> CreditApplications { get; set; } = [];
}
