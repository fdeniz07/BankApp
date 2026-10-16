using Domain.Enums;

namespace Application.Features.CreditApplications.Queries.GetList;

public class GetListCreditApplicationListItemDto
{
    public Guid Id { get; set; }
    public string ApplicationNumber { get; set; } = default!;
    public Guid CustomerId { get; set; }
    public Guid CreditSubTypeId { get; set; }
    public string CreditSubTypeName { get; set; } = default!;
    public string CurrencyCode { get; set; } = "USD";
    public decimal RequestedAmount { get; set; }
    public int RequestedTermMonths { get; set; }
    public decimal InterestRate { get; set; }
    public decimal? ApprovedInterestRate { get; set; }
    public CreditApplicationStatus Status { get; set; }
    public DateTimeOffset ApplicationDate { get; set; }
    public bool IsDeleted { get; set; }
}
