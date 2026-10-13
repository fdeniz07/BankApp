using FluentValidation;

namespace Application.Features.CreditApplications.Commands.Reject;

public class RejectCreditApplicationRequestValidator : AbstractValidator<RejectCreditApplicationRequest>
{
    public RejectCreditApplicationRequestValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.RejectionReason).NotEmpty().MaximumLength(500);
        RuleFor(c => c.DecisionBy).MaximumLength(100);
        RuleFor(c => c.Notes).MaximumLength(1000);
    }
}
