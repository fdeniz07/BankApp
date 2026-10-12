namespace Domain.Entities;

public sealed class CorporateCustomer : Customer
{
    public string CompanyName { get; set; } = default!;
    public string TaxNumber { get; set; } = default!;
    public string TaxOffice { get; set; } = default!;
    public string CompanyRegistrationNumber { get; set; } = default!; // Ticari sicil numarası
    public string? CompanyEmail { get; set; }
    public string AuthorizedPersonName { get; set; } = default!;
    public string? CompanyAddress { get; set; }
    public string? CompanyType { get; set; }
    public string? CompanySector { get; set; }
    public string? CompanyIndustry { get; set; }
    public string? CompanySubIndustry { get; set; }
    public DateTime? CompanyFoundationDate { get; set; }

}
