using FluentValidation;

namespace Application.Modules.Credit.Features.CreditApplications.Commands.Create;

public class CreateCreditApplicationRequestValidator : AbstractValidator<CreateCreditApplicationRequest>
{
    public CreateCreditApplicationRequestValidator()
    {
        // CustomerId may be empty for customers; the handler fills it from the current user.
        RuleFor(c => c.CreditSubTypeId).NotEmpty();
        RuleFor(c => c.RequestedAmount).GreaterThan(0);
        RuleFor(c => c.RequestedTermMonths).GreaterThan(0);
        RuleFor(c => c.Purpose).MaximumLength(500);
        RuleFor(c => c.Notes).MaximumLength(1000);
    }
}
