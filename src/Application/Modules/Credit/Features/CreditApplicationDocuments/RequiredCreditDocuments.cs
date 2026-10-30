using Domain.Enums;

namespace Application.Modules.Credit.Features.CreditApplicationDocuments;

/// <summary>Lab checklist for pending applications (soft gate / badges — not a hard block).</summary>
public static class RequiredCreditDocuments
{
    public static readonly string[] Types =
    [
        CreditDocumentType.IdentityDocument.Name,
        CreditDocumentType.IncomeProof.Name
    ];

    public static bool IsMissingRequired(IEnumerable<string> presentDocumentTypeNames)
    {
        HashSet<string> present = presentDocumentTypeNames
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Select(t => t.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return Types.Any(required => !present.Contains(required));
    }
}
