using FluentValidation;

namespace Application.Modules.Credit.Features.CreditApplications.Commands.Update;

public class UpdateCreditApplicationRequestValidator : AbstractValidator<UpdateCreditApplicationRequest>
{
    public UpdateCreditApplicationRequestValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.RequestedAmount).GreaterThan(0);
        RuleFor(c => c.RequestedTermMonths).GreaterThan(0);
        RuleFor(c => c.Purpose).MaximumLength(500);
        RuleFor(c => c.Notes).MaximumLength(1000);
    }
}
