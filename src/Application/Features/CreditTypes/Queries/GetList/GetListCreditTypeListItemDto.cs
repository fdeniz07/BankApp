using Domain.Enums;

namespace Application.Features.CreditTypes.Queries.GetList;

public class GetListCreditTypeListItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Code { get; set; } = default!;
    public string Locale { get; set; } = "tr";
    public CreditCategory Category { get; set; }
    public string CurrencyCode { get; set; } = default!;
    public CreditRiskLevel RiskLevel { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
}
