namespace Application.Modules.Credit.Features.CreditApplicationDocuments;

/// <summary>How missing required documents affect credit approval.</summary>
public enum ApproveDocumentGateMode
{
    /// <summary>No soft confirm and no hard block.</summary>
    None = 0,

    /// <summary>UI soft confirm + warning band; API still allows approve.</summary>
    Soft = 1,

    /// <summary>API and UI block approve until identity + income docs exist.</summary>
    Hard = 2
}
