using FluentValidation;

namespace Application.Modules.Credit.Features.CreditSubTypes.Commands.Update;

public class UpdateCreditSubTypeRequestValidator : AbstractValidator<UpdateCreditSubTypeRequest>
{
    public UpdateCreditSubTypeRequestValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.CreditTypeId).NotEmpty();
        RuleFor(c => c.Name).NotEmpty().MaximumLength(100);
        RuleFor(c => c.Code).NotEmpty().MaximumLength(50);
        RuleFor(c => c.Description).MaximumLength(500);
        RuleFor(c => c.MinAmount).GreaterThan(0);
        RuleFor(c => c.MaxAmount).GreaterThan(0);
        RuleFor(c => c.InterestRate).GreaterThanOrEqualTo(0);
        RuleFor(c => c.MinTermMonths).GreaterThan(0);
        RuleFor(c => c.MaxTermMonths).GreaterThan(0);
        RuleFor(c => c.GracePeriodDays).GreaterThanOrEqualTo(0);
    }
}
