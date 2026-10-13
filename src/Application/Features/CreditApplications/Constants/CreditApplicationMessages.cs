namespace Application.Features.CreditApplications.Constants;

public static class CreditApplicationMessages
{
    public const string NotExists = "Credit application does not exist.";
    public const string CustomerNotExists = "Customer does not exist.";
    public const string CreditSubTypeNotExists = "Credit sub type does not exist.";
    public const string CreditSubTypeNotActive = "Credit sub type is not active.";
    public const string CustomerNotActive = "Customer is not active.";
    public const string CategoryMismatch = "Customer type does not match credit category.";
    public const string AmountOutOfRange = "Requested amount is out of allowed range.";
    public const string TermOutOfRange = "Requested term is out of allowed range.";
    public const string ApplicationNumberAlreadyExists = "Application number already exists.";
    public const string OnlyPendingCanBeDecided = "Only pending applications can be approved or rejected.";
    public const string OnlyPendingCanBeUpdated = "Only pending applications can be updated.";
}
