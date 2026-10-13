namespace Application.Features.CreditSubTypes.Commands.Create;

public class CreatedCreditSubTypeResponse
{
    public Guid Id { get; set; }
    public Guid CreditTypeId { get; set; }
    public string Name { get; set; } = default!;
    public string Code { get; set; } = default!;
    public string? Description { get; set; }
    public decimal MinAmount { get; set; }
    public decimal MaxAmount { get; set; }
    public decimal InterestRate { get; set; }
    public int MinTermMonths { get; set; }
    public int MaxTermMonths { get; set; }
    public decimal? LateFeeRate { get; set; }
    public decimal? EarlyPaymentFeeRate { get; set; }
    public decimal? ProcessingFeeAmount { get; set; }
    public decimal? ProcessingFeeRate { get; set; }
    public int GracePeriodDays { get; set; }
    public bool RequiresCollateral { get; set; }
    public bool RequiresIncomeDocument { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
}
