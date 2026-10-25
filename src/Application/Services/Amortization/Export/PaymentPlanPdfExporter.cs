using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Application.Services.Amortization.Export;

public sealed class PaymentPlanPdfExporter : IPaymentPlanDocumentExporter
{
    static PaymentPlanPdfExporter()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public PaymentPlanExportFormat Format => PaymentPlanExportFormat.Pdf;

    public PaymentPlanExportResult Export(PaymentPlanExportDocument document)
    {
        string stamp = document.GeneratedAtUtc.ToString("yyyyMMdd-HHmm");
        string fileBase = string.IsNullOrWhiteSpace(document.ApplicationNumber)
            ? $"payment-plan-{stamp}"
            : $"payment-plan-{Sanitize(document.ApplicationNumber)}-{stamp}";

        byte[] bytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.5f, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Column(col =>
                {
                    col.Item().Text("BankApp").Bold().FontSize(16).FontColor(Colors.Teal.Darken2);
                    col.Item().Text("Payment schedule / Ödeme planı").FontSize(12);
                    col.Item().PaddingTop(4).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);
                });

                page.Content().PaddingVertical(10).Column(col =>
                {
                    if (!string.IsNullOrWhiteSpace(document.ApplicationNumber))
                        col.Item().Text($"Application: {document.ApplicationNumber}");
                    if (!string.IsNullOrWhiteSpace(document.CustomerName))
                        col.Item().Text($"Customer: {document.CustomerName}");

                    col.Item().PaddingTop(6).Text(
                        $"Amount: {document.Amount:N2} {document.CurrencyCode}  ·  Term: {document.TermMonths} months  ·  Rate: {document.InterestRate:N2}%");
                    col.Item().Text(
                        $"Monthly: {document.MonthlyPayment:N2}  ·  Total: {document.TotalPayment:N2}  ·  Interest: {document.TotalInterest:N2}");
                    col.Item().PaddingBottom(8).Text(
                        $"Generated (UTC): {document.GeneratedAtUtc:yyyy-MM-dd HH:mm}");

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(36);
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(HeaderCell).Text("Month");
                            header.Cell().Element(HeaderCell).AlignRight().Text("Installment");
                            header.Cell().Element(HeaderCell).AlignRight().Text("Principal");
                            header.Cell().Element(HeaderCell).AlignRight().Text("Interest");
                            header.Cell().Element(HeaderCell).AlignRight().Text("Balance");
                        });

                        foreach (PaymentPlanInstallment row in document.Installments)
                        {
                            table.Cell().Element(BodyCell).Text(row.Month.ToString());
                            table.Cell().Element(BodyCell).AlignRight().Text($"{row.Payment:N2}");
                            table.Cell().Element(BodyCell).AlignRight().Text($"{row.Principal:N2}");
                            table.Cell().Element(BodyCell).AlignRight().Text($"{row.Interest:N2}");
                            table.Cell().Element(BodyCell).AlignRight().Text($"{row.RemainingBalance:N2}");
                        }
                    });
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Page ");
                    text.CurrentPageNumber();
                    text.Span(" / ");
                    text.TotalPages();
                });
            });
        }).GeneratePdf();

        return new PaymentPlanExportResult
        {
            Content = bytes,
            ContentType = "application/pdf",
            FileName = $"{fileBase}.pdf",
            Format = PaymentPlanExportFormat.Pdf
        };
    }

    private static IContainer HeaderCell(IContainer container) =>
        container.DefaultTextStyle(x => x.SemiBold().FontSize(8))
            .PaddingVertical(4)
            .BorderBottom(1)
            .BorderColor(Colors.Grey.Medium)
            .Background(Colors.Grey.Lighten3)
            .PaddingHorizontal(3);

    private static IContainer BodyCell(IContainer container) =>
        container.BorderBottom(0.5f)
            .BorderColor(Colors.Grey.Lighten2)
            .PaddingVertical(2)
            .PaddingHorizontal(3);

    private static string Sanitize(string value)
    {
        char[] invalid = Path.GetInvalidFileNameChars();
        return new string(value.Select(c => invalid.Contains(c) ? '-' : c).ToArray());
    }
}
