namespace Application.Modules.Credit.Features.CreditApplications.ApprovalAuthority;

/// <summary>
/// Delegation of authority: max approve amount by staff position (lab/prod config).
/// </summary>
public sealed class ApprovalAuthorityOptions
{
    public const string SectionName = "ApprovalAuthority";

    public bool Enabled { get; set; }

    /// <summary>Position name → max approved amount (same currency as application package).</summary>
    public Dictionary<string, decimal> LimitsByPosition { get; set; } =
        new(StringComparer.OrdinalIgnoreCase);

    /// <summary>ASP.NET Identity role names that bypass position limits.</summary>
    public string[] UnlimitedRoles { get; set; } = ["BankManager", "SuperAdmin"];
}
