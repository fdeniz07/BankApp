using Domain.Enums;

namespace Application.Modules.Credit.Features.CreditTypes.Commands.Update;

public class UpdatedCreditTypeResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Code { get; set; } = default!;
    public string Locale { get; set; } = "tr";
    public CreditCategory Category { get; set; } = null!;
    public string? Description { get; set; }
    public string CurrencyCode { get; set; } = default!;
    public CreditRiskLevel RiskLevel { get; set; } = null!;
    public bool RequiresCollateral { get; set; }
    public bool RequiresGuarantor { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
    public DateTimeOffset? UpdatedDate { get; set; }
}
