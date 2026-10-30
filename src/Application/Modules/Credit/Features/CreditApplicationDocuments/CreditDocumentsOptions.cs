namespace Application.Modules.Credit.Features.CreditApplicationDocuments;

/// <summary>
/// Credit document UX flags (lab vs prod soft-approve gate).
/// </summary>
public sealed class CreditDocumentsOptions
{
    public const string SectionName = "CreditDocuments";

    /// <summary>
    /// When true, staff get a soft confirm + warning band if identity/income docs are missing on approve.
    /// List badges and missing-docs filter stay available either way.
    /// </summary>
    public bool SoftApproveGateEnabled { get; set; } = true;
}
