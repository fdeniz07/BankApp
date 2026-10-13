using Core.Repositories;
using Domain.Enums;

namespace Domain.Entities;

public class CreditType : Entity
{
    public string Name { get; set; } = default!;
    public string Code { get; set; } = default!;
    public CreditCategory Category { get; set; } = CreditCategory.Individual;
    public string? Description { get; set; }
    public string CurrencyCode { get; set; } = "TRY";
    public CreditRiskLevel RiskLevel { get; set; } = CreditRiskLevel.Medium;
    public bool RequiresCollateral { get; set; }
    public bool RequiresGuarantor { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<CreditSubType> CreditSubTypes { get; set; } = [];
}
