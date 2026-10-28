namespace Application.Modules.Credit.Features.CreditApplicationDocuments.Constants;

public static class CreditApplicationDocumentMessages
{
    public const string NotExists = "Credit application document not found.";
    public const string EmptyFile = "Document file is required.";
    public const string TooLarge = "Document must be at most 5 MB.";
    public const string InvalidContentType = "Document must be PDF, JPEG, or PNG.";
    public const string InvalidContent = "Document content does not match the declared type.";
    public const string InvalidDocumentType = "Document type is invalid.";
    public const string OnlyPendingCanUpload = "Documents can only be uploaded while the application is Pending.";
    public const string MaxDocumentsExceeded = "At most 10 documents are allowed per application.";
}
