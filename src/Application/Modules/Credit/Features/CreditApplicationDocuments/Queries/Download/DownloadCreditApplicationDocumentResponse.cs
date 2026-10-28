namespace Application.Modules.Credit.Features.CreditApplicationDocuments.Queries.Download;

public class DownloadCreditApplicationDocumentResponse
{
    public Stream Content { get; set; } = Stream.Null;
    public string ContentType { get; set; } = default!;
    public string FileName { get; set; } = default!;
}
