using Domain.Enums;

namespace Application.Features.CreditTypes.Queries.GetById;

public class GetByIdCreditTypeResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Code { get; set; } = default!;
    public string Locale { get; set; } = "tr";
    public CreditCategory Category { get; set; }
    public string? Description { get; set; }
    public string CurrencyCode { get; set; } = default!;
    public CreditRiskLevel RiskLevel { get; set; }
    public bool RequiresCollateral { get; set; }
    public bool RequiresGuarantor { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
    public DateTimeOffset? UpdatedDate { get; set; }
}
