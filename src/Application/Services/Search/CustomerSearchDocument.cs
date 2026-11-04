namespace Application.Services.Search;

public sealed class CustomerSearchDocument
{
    public Guid Id { get; set; }
    public string Kind { get; set; } = ""; // Individual | Corporate
    public string CustomerNumber { get; set; } = "";
    public string Email { get; set; } = "";
    public string PhoneNumber { get; set; } = "";
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? NationalId { get; set; }
    public string? CompanyName { get; set; }
    public string? TaxNumber { get; set; }
    public string? AuthorizedPersonName { get; set; }
    public string? BranchId { get; set; }
    public string? BranchCode { get; set; }
    public string? BranchName { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
}

public static class CustomerSearchKinds
{
    public const string Individual = "Individual";
    public const string Corporate = "Corporate";
}
