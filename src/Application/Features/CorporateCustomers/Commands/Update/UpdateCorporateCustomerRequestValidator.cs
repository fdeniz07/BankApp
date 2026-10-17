using Application.Common.Validation;
using FluentValidation;

namespace Application.Features.CorporateCustomers.Commands.Update;

public class UpdateCorporateCustomerRequestValidator : AbstractValidator<UpdateCorporateCustomerRequest>
{
    public UpdateCorporateCustomerRequestValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.CustomerNumber).NotEmpty().MaximumLength(50);
        RuleFor(c => c.Email).NotEmpty().EmailAddress().MaximumLength(100)
            .MustNotContainTurkishCharacters();
        RuleFor(c => c.PhoneNumber).NotEmpty().MaximumLength(20);
        RuleFor(c => c.Address).NotEmpty().MaximumLength(500);
        RuleFor(c => c.CompanyName).NotEmpty().MaximumLength(200);
        RuleFor(c => c.TaxNumber).NotEmpty().MaximumLength(20);
        RuleFor(c => c.TaxOffice).NotEmpty().MaximumLength(100);
        RuleFor(c => c.CompanyRegistrationNumber).NotEmpty().MaximumLength(50);
        RuleFor(c => c.AuthorizedPersonName).NotEmpty().MaximumLength(100);
    }
}
