namespace Application.Features.Dashboard.Queries.GetSummary;

public class GetDashboardSummaryResponse
{
    public string Scope { get; set; } = default!;

    public int? CustomerTotal { get; set; }
    public int? IndividualActive { get; set; }
    public int? CorporateActive { get; set; }
    public int? CustomerInactive { get; set; }

    public int ApplicationTotal { get; set; }
    public int ApplicationPending { get; set; }
    public int ApplicationApproved { get; set; }
    public int ApplicationRejected { get; set; }
    public int ApplicationCancelled { get; set; }
}
