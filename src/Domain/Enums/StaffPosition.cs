using Ardalis.SmartEnum;

namespace Domain.Enums;

/// <summary>Fixed bank staff positions (UI labels via i18n).</summary>
public sealed class StaffPosition : SmartEnum<StaffPosition>
{
    public static readonly StaffPosition CreditAnalyst = new(nameof(CreditAnalyst), 1);
    public static readonly StaffPosition RelationshipManager = new(nameof(RelationshipManager), 2);
    public static readonly StaffPosition BranchOfficer = new(nameof(BranchOfficer), 3);
    public static readonly StaffPosition RiskOfficer = new(nameof(RiskOfficer), 4);
    public static readonly StaffPosition OperationsSpecialist = new(nameof(OperationsSpecialist), 5);
    public static readonly StaffPosition BranchManager = new(nameof(BranchManager), 6);

    private StaffPosition(string name, int value) : base(name, value)
    {
    }

    public static bool IsValidName(string? value) =>
        !string.IsNullOrWhiteSpace(value) &&
        TryFromName(value.Trim(), ignoreCase: true, out _);

    public static string Normalize(string value) =>
        FromName(value.Trim(), ignoreCase: true).Name;
}
