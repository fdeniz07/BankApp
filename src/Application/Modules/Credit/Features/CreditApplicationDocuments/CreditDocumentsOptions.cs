namespace Application.Modules.Credit.Features.CreditApplicationDocuments;

/// <summary>
/// Credit document approval gate (lab vs prod).
/// </summary>
public sealed class CreditDocumentsOptions
{
    public const string SectionName = "CreditDocuments";

    /// <summary>
    /// <see cref="ApproveDocumentGateMode.None"/> | Soft | Hard.
    /// List badges and missing-docs filter stay available in all modes.
    /// </summary>
    public ApproveDocumentGateMode ApproveGateMode { get; set; } = ApproveDocumentGateMode.Hard;
}
