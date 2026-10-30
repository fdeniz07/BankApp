using Core.Repositories;

namespace Domain.Entities;

/// <summary>User-owned approve/reject letter template (system defaults remain in client i18n).</summary>
public class DecisionLetterTemplate : Entity
{
    public Guid UserId { get; set; }
    /// <summary>Approve | Reject</summary>
    public string Kind { get; set; } = "Approve";
    /// <summary>tr | en | de</summary>
    public string Locale { get; set; } = "tr";
    public string Name { get; set; } = default!;
    public string Body { get; set; } = default!;
}
