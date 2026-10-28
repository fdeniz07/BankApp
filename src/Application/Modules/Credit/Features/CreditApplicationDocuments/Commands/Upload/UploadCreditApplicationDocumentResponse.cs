namespace Application.Modules.Credit.Features.CreditApplicationDocuments.Commands.Upload;

public class UploadCreditApplicationDocumentResponse
{
    public Guid Id { get; set; }
    public Guid CreditApplicationId { get; set; }
    public string DocumentType { get; set; } = default!;
    public string OriginalFileName { get; set; } = default!;
    public string ContentType { get; set; } = default!;
    public long SizeBytes { get; set; }
    public DateTimeOffset UploadedAt { get; set; }
}
