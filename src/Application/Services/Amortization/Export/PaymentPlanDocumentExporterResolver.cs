using Core.CrossCuttingConcerns.Exceptions.Types;

namespace Application.Services.Amortization.Export;

public sealed class PaymentPlanDocumentExporterResolver : IPaymentPlanDocumentExporterResolver
{
    private readonly IReadOnlyDictionary<PaymentPlanExportFormat, IPaymentPlanDocumentExporter> _exporters;

    public PaymentPlanDocumentExporterResolver(IEnumerable<IPaymentPlanDocumentExporter> exporters)
    {
        _exporters = exporters.ToDictionary(e => e.Format);
    }

    public IPaymentPlanDocumentExporter Resolve(PaymentPlanExportFormat format)
    {
        if (_exporters.TryGetValue(format, out IPaymentPlanDocumentExporter? exporter))
            return exporter;

        throw new BusinessException($"Payment plan export format '{format}' is not supported.");
    }

    public bool TryParseFormat(string? raw, out PaymentPlanExportFormat format)
    {
        format = default;
        if (string.IsNullOrWhiteSpace(raw))
            return false;

        return Enum.TryParse(raw.Trim(), ignoreCase: true, out format)
            && Enum.IsDefined(format)
            && _exporters.ContainsKey(format);
    }
}
