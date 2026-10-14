using Core.Security.Entities;

namespace Domain.Entities;

public class ApplicationUser : User
{
    public Guid? CustomerId { get; set; }

    public Customer? Customer { get; set; }
}
