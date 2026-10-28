namespace Application.Services.CreditDocuments;

public interface ICreditDocumentStorage
{
    /// <summary>Persists the stream; returns a relative storage key (not a public URL).</summary>
    Task<string> SaveAsync(
        Guid applicationId,
        Guid documentId,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default);

    Task<Stream> OpenReadAsync(string storagePath, CancellationToken cancellationToken = default);

    Task DeleteIfExistsAsync(string? storagePath, CancellationToken cancellationToken = default);
}
