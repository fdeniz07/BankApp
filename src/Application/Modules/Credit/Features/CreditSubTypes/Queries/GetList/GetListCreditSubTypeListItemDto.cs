namespace Application.Modules.Credit.Features.CreditSubTypes.Queries.GetList;

public class GetListCreditSubTypeListItemDto
{
    public Guid Id { get; set; }
    public Guid CreditTypeId { get; set; }
    public string Name { get; set; } = default!;
    public string Code { get; set; } = default!;
    public string Locale { get; set; } = "tr";
    public string Category { get; set; } = "Individual";
    public string CurrencyCode { get; set; } = "USD";
    public decimal MinAmount { get; set; }
    public decimal MaxAmount { get; set; }
    public decimal InterestRate { get; set; }
    public int MinTermMonths { get; set; }
    public int MaxTermMonths { get; set; }
    public List<decimal> AmountOptions { get; set; } = [];
    public List<int> TermOptions { get; set; } = [];
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
}
