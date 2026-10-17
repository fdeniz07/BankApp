using Application.Common.Validation;
using Domain.Enums;
using FluentValidation;

namespace Application.Features.StaffUsers.Commands.Update;

public class UpdateStaffUserRequestValidator : AbstractValidator<UpdateStaffUserRequest>
{
    public UpdateStaffUserRequestValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.Email).NotEmpty().EmailAddress().MaximumLength(100)
            .MustNotContainTurkishCharacters();
        RuleFor(c => c.NewPassword)
            .MinimumLength(6)
            .MaximumLength(100)
            .When(c => !string.IsNullOrWhiteSpace(c.NewPassword));
        RuleFor(c => c.FirstName).MaximumLength(50);
        RuleFor(c => c.LastName).MaximumLength(50);
        RuleFor(c => c.Title)
            .Must(title => title is null || PersonTitle.List.Contains(title))
            .WithMessage("Title is invalid.");
        RuleFor(c => c.Gender)
            .Must(gender => gender is null || Gender.List.Contains(gender))
            .WithMessage("Gender is invalid.");
        RuleFor(c => c.Position)
            .Must(position => position is null || StaffPosition.List.Contains(position))
            .WithMessage("Position is invalid.");
        RuleFor(c => c.DateOfBirth)
            .Must(d => d is null || d <= DateOnly.FromDateTime(DateTime.UtcNow.Date))
            .WithMessage("Date of birth cannot be in the future.");
    }
}
