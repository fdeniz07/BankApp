namespace Application.Features.CreditSubTypes.Queries.GetList;

public class GetListCreditSubTypeListItemDto
{
    public Guid Id { get; set; }
    public Guid CreditTypeId { get; set; }
    public string Name { get; set; } = default!;
    public string Code { get; set; } = default!;
    public decimal MinAmount { get; set; }
    public decimal MaxAmount { get; set; }
    public decimal InterestRate { get; set; }
    public int MinTermMonths { get; set; }
    public int MaxTermMonths { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
}
