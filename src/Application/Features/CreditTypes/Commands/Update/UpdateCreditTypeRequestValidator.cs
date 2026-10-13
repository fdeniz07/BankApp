using Domain.Enums;
using FluentValidation;

namespace Application.Features.CreditTypes.Commands.Update;

public class UpdateCreditTypeRequestValidator : AbstractValidator<UpdateCreditTypeRequest>
{
    public UpdateCreditTypeRequestValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.Name).NotEmpty().MaximumLength(100);
        RuleFor(c => c.Code).NotEmpty().MaximumLength(50);
        RuleFor(c => c.Category)
            .NotNull()
            .Must(category => CreditCategory.TryFromName(category.Name, true, out _))
            .WithMessage("Invalid credit category.");
        RuleFor(c => c.Description).MaximumLength(500);
        RuleFor(c => c.CurrencyCode).NotEmpty().Length(3);
        RuleFor(c => c.RiskLevel)
            .NotNull()
            .Must(riskLevel => CreditRiskLevel.TryFromName(riskLevel.Name, true, out _))
            .WithMessage("Invalid credit risk level.");
    }
}
