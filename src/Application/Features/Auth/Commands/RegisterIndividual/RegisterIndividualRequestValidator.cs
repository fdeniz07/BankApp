using Application.Common.Validation;
using Application.Features.Customers.Constants;
using Domain.Enums;
using FluentValidation;

namespace Application.Features.Auth.Commands.RegisterIndividual;

public class RegisterIndividualRequestValidator : AbstractValidator<RegisterIndividualRequest>
{
    public RegisterIndividualRequestValidator()
    {
        RuleFor(r => r.Email).NotEmpty().EmailAddress().MaximumLength(100)
            .MustNotContainTurkishCharacters();
        RuleFor(r => r.Password).NotEmpty().MinimumLength(6).MaximumLength(100);
        RuleFor(r => r.PhoneNumber).NotEmpty().MaximumLength(20);
        RuleFor(r => r.Address).NotEmpty().MaximumLength(500);
        RuleFor(r => r.FirstName).NotEmpty().MaximumLength(50);
        RuleFor(r => r.LastName).NotEmpty().MaximumLength(50);
        RuleFor(r => r.NationalId).NotEmpty().Length(11);
        RuleFor(r => r.Gender)
            .NotEmpty()
            .Must(Gender.IsValidName)
            .WithMessage(CustomerLifecycleMessages.InvalidGender);
    }
}
