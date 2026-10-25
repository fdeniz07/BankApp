using Domain.Enums;

namespace Application.Modules.Credit.Features.CreditTypes.Queries.GetList;

public class GetListCreditTypeListItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Code { get; set; } = default!;
    public string Locale { get; set; } = "tr";
    public CreditCategory Category { get; set; } = null!;
    public string CurrencyCode { get; set; } = default!;
    public CreditRiskLevel RiskLevel { get; set; } = null!;
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
}
