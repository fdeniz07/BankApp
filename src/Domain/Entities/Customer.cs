using Core.Repositories;

namespace Domain.Entities;

public abstract class Customer : Entity
{
    public string CustomerNumber { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string PhoneNumber { get; set; } = default!;
    public string Address { get; set; } = default!;
    public bool IsActive { get; set; } = true;

    public ICollection<CreditApplication> CreditApplications { get; set; } = [];
}
