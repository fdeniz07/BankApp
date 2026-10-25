namespace Application.Services.Amortization.Export;

public interface IPaymentPlanDocumentExporterResolver
{
    IPaymentPlanDocumentExporter Resolve(PaymentPlanExportFormat format);

    bool TryParseFormat(string? raw, out PaymentPlanExportFormat format);
}
