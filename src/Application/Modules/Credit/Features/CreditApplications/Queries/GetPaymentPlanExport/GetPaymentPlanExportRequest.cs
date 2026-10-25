using Application.Services.Amortization.Export;
using Core.Application.Pipelines.Authorization;
using Core.Security.Constants;
using MediatR;

namespace Application.Modules.Credit.Features.CreditApplications.Queries.GetPaymentPlanExport;

public class GetPaymentPlanExportRequest : IRequest<PaymentPlanExportResult>, ISecuredRequest
{
    public string[] Roles => [GeneralOperationClaims.Customer, GeneralOperationClaims.BankStaff];

    /// <summary>Export format name (pdf, …). Parsed by the exporter resolver.</summary>
    public string Format { get; set; } = "pdf";

    public Guid? CreditSubTypeId { get; set; }
    public decimal Amount { get; set; }
    public int TermMonths { get; set; }
    public decimal? InterestRate { get; set; }

    /// <summary>UI locale (tr / en / de) for PDF labels and number format.</summary>
    public string? Locale { get; set; }

    /// <summary>Optional header metadata (from application detail UI).</summary>
    public string? ApplicationNumber { get; set; }
    public string? CustomerName { get; set; }
}
