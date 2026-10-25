namespace Application.Services.Amortization.Export;

public sealed class PaymentPlanExportResult
{
    public required byte[] Content { get; init; }
    public required string ContentType { get; init; }
    public required string FileName { get; init; }
    public required PaymentPlanExportFormat Format { get; init; }
}
