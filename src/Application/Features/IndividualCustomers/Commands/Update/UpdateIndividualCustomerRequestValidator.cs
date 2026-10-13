using FluentValidation;

namespace Application.Features.IndividualCustomers.Commands.Update;

public class UpdateIndividualCustomerRequestValidator : AbstractValidator<UpdateIndividualCustomerRequest>
{
    public UpdateIndividualCustomerRequestValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.CustomerNumber).NotEmpty().MaximumLength(50);
        RuleFor(c => c.Email).NotEmpty().EmailAddress().MaximumLength(100);
        RuleFor(c => c.PhoneNumber).NotEmpty().MaximumLength(20);
        RuleFor(c => c.Address).NotEmpty().MaximumLength(500);
        RuleFor(c => c.FirstName).NotEmpty().MaximumLength(50);
        RuleFor(c => c.LastName).NotEmpty().MaximumLength(50);
        RuleFor(c => c.NationalId).NotEmpty().Length(11);
    }
}
