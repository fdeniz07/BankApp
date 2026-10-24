namespace Application.Modules.Customers.Features.IndividualCustomers.Queries.GetList;

public class GetListIndividualCustomerListItemDto
{
    public Guid Id { get; set; }
    public string CustomerNumber { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string PhoneNumber { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string NationalId { get; set; } = default!;
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
}
