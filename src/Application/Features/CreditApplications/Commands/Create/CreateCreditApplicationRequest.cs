using MediatR;

namespace Application.Features.CreditApplications.Commands.Create;

public class CreateCreditApplicationRequest : IRequest<CreatedCreditApplicationResponse>
{
    public Guid CustomerId { get; set; }
    public Guid CreditSubTypeId { get; set; }
    public decimal RequestedAmount { get; set; }
    public int RequestedTermMonths { get; set; }
    public decimal? MonthlyIncome { get; set; }
    public decimal? ExistingDebtAmount { get; set; }
    public string? Purpose { get; set; }
    public string? Notes { get; set; }
}
