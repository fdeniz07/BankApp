namespace Application.Services.Amortization.Export;

public interface IPaymentPlanDocumentExporter
{
    PaymentPlanExportFormat Format { get; }

    PaymentPlanExportResult Export(PaymentPlanExportDocument document);
}
