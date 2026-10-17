using FluentValidation;

namespace Application.Features.CreditTypes.Commands.SetActive;

public class SetCreditTypeActiveRequestValidator : AbstractValidator<SetCreditTypeActiveRequest>
{
    public SetCreditTypeActiveRequestValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
    }
}
