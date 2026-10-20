namespace Application.Features.CorporateCustomers.Queries.GetCatalog;

public class GetCorporateCustomerCatalogResponse
{
    public IReadOnlyList<string> CompanyTypes { get; set; } = [];
    public IReadOnlyList<string> CompanySectors { get; set; } = [];
    public IReadOnlyList<string> CompanyIndustries { get; set; } = [];
    public IReadOnlyList<string> CompanySubIndustries { get; set; } = [];
}
