namespace Application.Modules.Customers.Features.CorporateCustomers.Queries.GetList;

public class GetListCorporateCustomerListItemDto
{
    public Guid Id { get; set; }
    public string CustomerNumber { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string PhoneNumber { get; set; } = default!;
    public string CompanyName { get; set; } = default!;
    public string TaxNumber { get; set; } = default!;
    public string AuthorizedPersonName { get; set; } = default!;
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
}
