namespace Application.Services.Search;

public sealed class CreditTypeSearchDocument
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Code { get; set; } = "";
    public string Locale { get; set; } = "tr";
    public string Category { get; set; } = "";
    public string? Description { get; set; }
    public string CurrencyCode { get; set; } = "TRY";
    public string RiskLevel { get; set; } = "";
    public bool RequiresCollateral { get; set; }
    public bool RequiresGuarantor { get; set; }
    public bool IsActive { get; set; }
}
