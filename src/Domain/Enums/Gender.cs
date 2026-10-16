using Ardalis.SmartEnum;

namespace Domain.Enums;

public sealed class Gender : SmartEnum<Gender>
{
    public static readonly Gender Female = new(nameof(Female), 1);
    public static readonly Gender Male = new(nameof(Male), 2);
    public static readonly Gender PreferNotToSay = new(nameof(PreferNotToSay), 3);

    private Gender(string name, int value) : base(name, value)
    {
    }

    public static bool IsValidName(string? value) =>
        !string.IsNullOrWhiteSpace(value) &&
        TryFromName(value.Trim(), ignoreCase: true, out _);

    public static string Normalize(string value) =>
        FromName(value.Trim(), ignoreCase: true).Name;
}
