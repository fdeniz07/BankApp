namespace Application.Modules.Credit.Features.CreditApplicationDocuments.Queries.GetList;

public class GetListCreditApplicationDocumentItemDto
{
    public Guid Id { get; set; }
    public string DocumentType { get; set; } = default!;
    public string OriginalFileName { get; set; } = default!;
    public string ContentType { get; set; } = default!;
    public long SizeBytes { get; set; }
    public DateTimeOffset UploadedAt { get; set; }
}
