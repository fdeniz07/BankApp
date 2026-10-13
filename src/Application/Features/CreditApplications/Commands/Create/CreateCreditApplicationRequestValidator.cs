using FluentValidation;

namespace Application.Features.CreditApplications.Commands.Create;

public class CreateCreditApplicationRequestValidator : AbstractValidator<CreateCreditApplicationRequest>
{
    public CreateCreditApplicationRequestValidator()
    {
        RuleFor(c => c.CustomerId).NotEmpty();
        RuleFor(c => c.CreditSubTypeId).NotEmpty();
        RuleFor(c => c.RequestedAmount).GreaterThan(0);
        RuleFor(c => c.RequestedTermMonths).GreaterThan(0);
        RuleFor(c => c.Purpose).MaximumLength(500);
        RuleFor(c => c.Notes).MaximumLength(1000);
    }
}
