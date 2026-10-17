using System.Text.RegularExpressions;
using FluentValidation;

namespace Application.Common.Validation;

public static partial class EmailRules
{
    public const string NoTurkishCharsMessage =
        "Email must not contain Turkish characters (ı, İ, ğ, Ğ, ü, Ü, ş, Ş, ö, Ö, ç, Ç).";

    public static IRuleBuilderOptions<T, string> MustNotContainTurkishCharacters<T>(
        this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .Must(email => email is null || !TurkishCharRegex().IsMatch(email))
            .WithMessage(NoTurkishCharsMessage);
    }

    public static bool ContainsTurkishCharacters(string? email) =>
        !string.IsNullOrEmpty(email) && TurkishCharRegex().IsMatch(email);

    [GeneratedRegex(@"[ıİğĞüÜşŞöÖçÇ]")]
    private static partial Regex TurkishCharRegex();
}
