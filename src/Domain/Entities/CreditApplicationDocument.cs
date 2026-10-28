using Core.Repositories;
using Domain.Enums;

namespace Domain.Entities;

/// <summary>
/// File attached to a credit application (identity, income proof, etc.). Stored on disk; metadata in SQL.
/// </summary>
public class CreditApplicationDocument : Entity
{
    public Guid CreditApplicationId { get; set; }
    public CreditDocumentType DocumentType { get; set; } = CreditDocumentType.Other;
    public string OriginalFileName { get; set; } = default!;
    public string ContentType { get; set; } = default!;
    public long SizeBytes { get; set; }
    /// <summary>Relative path under App_Data/credit-documents (not publicly served).</summary>
    public string StoragePath { get; set; } = default!;
    public Guid UploadedByUserId { get; set; }
    public DateTimeOffset UploadedAt { get; set; }

    public CreditApplication CreditApplication { get; set; } = default!;
}
