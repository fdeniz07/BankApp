using Application.Common.Validation;
using Application.Modules.Customers.Features.Customers.Constants;
using Domain.Enums;
using FluentValidation;

namespace Application.Modules.Customers.Features.IndividualCustomers.Commands.Create;

public class CreateIndividualCustomerRequestValidator : AbstractValidator<CreateIndividualCustomerRequest>
{
    public CreateIndividualCustomerRequestValidator()
    {
        RuleFor(c => c.Email).NotEmpty().EmailAddress().MaximumLength(100)
            .MustNotContainTurkishCharacters();
        RuleFor(c => c.Password).NotEmpty().MinimumLength(6).MaximumLength(100);
        RuleFor(c => c.PhoneNumber).NotEmpty().MaximumLength(20);
        RuleFor(c => c.Address).NotEmpty().MaximumLength(500);
        RuleFor(c => c.FirstName).NotEmpty().MaximumLength(50);
        RuleFor(c => c.LastName).NotEmpty().MaximumLength(50);
        RuleFor(c => c.NationalId).NotEmpty().Length(11);
        RuleFor(c => c.Gender)
            .NotEmpty()
            .Must(Gender.IsValidName)
            .WithMessage(CustomerLifecycleMessages.InvalidGender);
    }
}
