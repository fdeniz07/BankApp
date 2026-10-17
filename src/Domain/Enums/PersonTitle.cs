using Ardalis.SmartEnum;

namespace Domain.Enums;

/// <summary>Academic / professional title (not gender).</summary>
public sealed class PersonTitle : SmartEnum<PersonTitle>
{
    public static readonly PersonTitle Dr = new(nameof(Dr), 1);
    public static readonly PersonTitle Prof = new(nameof(Prof), 2);

    private PersonTitle(string name, int value) : base(name, value)
    {
    }

    public static bool IsValidName(string? value) => !string.IsNullOrWhiteSpace(value) && TryFromName(value.Trim(), ignoreCase: true, out _);

    public static string Normalize(string value) => FromName(value.Trim(), ignoreCase: true).Name;
}
