using Ardalis.SmartEnum;

namespace Domain.Enums;

public sealed class CreditDocumentType : SmartEnum<CreditDocumentType>
{
    public static readonly CreditDocumentType IdentityDocument = new(nameof(IdentityDocument), 1);
    public static readonly CreditDocumentType IncomeProof = new(nameof(IncomeProof), 2);
    public static readonly CreditDocumentType Other = new(nameof(Other), 3);

    private CreditDocumentType(string name, int value) : base(name, value)
    {
    }
}
