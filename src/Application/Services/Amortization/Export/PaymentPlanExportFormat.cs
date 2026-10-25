namespace Application.Services.Amortization.Export;

/// <summary>
/// Supported payment-plan download formats. Add new values when exporters are registered.
/// </summary>
public enum PaymentPlanExportFormat
{
    Pdf = 1
    // Future: Csv = 2, Xlsx = 3
}
