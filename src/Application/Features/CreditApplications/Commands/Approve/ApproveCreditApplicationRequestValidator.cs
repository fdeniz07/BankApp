using FluentValidation;

namespace Application.Features.CreditApplications.Commands.Approve;

public class ApproveCreditApplicationRequestValidator : AbstractValidator<ApproveCreditApplicationRequest>
{
    public ApproveCreditApplicationRequestValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.ApprovedAmount).GreaterThan(0);
        RuleFor(c => c.ApprovedTermMonths).GreaterThan(0);
        RuleFor(c => c.ApprovedInterestRate).GreaterThanOrEqualTo(0);
        RuleFor(c => c.DecisionBy).MaximumLength(100);
        RuleFor(c => c.Notes).MaximumLength(1000);
    }
}
