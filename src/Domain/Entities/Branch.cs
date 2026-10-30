using Core.Repositories;

namespace Domain.Entities;

/// <summary>Bank branch (şube) — staff assignment and decision snapshot source.</summary>
public class Branch : Entity
{
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? City { get; set; }
    public bool IsActive { get; set; } = true;
}
