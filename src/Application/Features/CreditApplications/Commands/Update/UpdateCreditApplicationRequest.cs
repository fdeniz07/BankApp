using MediatR;

namespace Application.Features.CreditApplications.Commands.Update;

public class UpdateCreditApplicationRequest : IRequest<UpdatedCreditApplicationResponse>
{
    public Guid Id { get; set; }
    public decimal RequestedAmount { get; set; }
    public int RequestedTermMonths { get; set; }
    public decimal? MonthlyIncome { get; set; }
    public decimal? ExistingDebtAmount { get; set; }
    public string? Purpose { get; set; }
    public string? Notes { get; set; }
}
