using FluentValidation;

namespace Application.Features.Auth.Commands.ConfirmEmail;

public class ConfirmEmailRequestValidator : AbstractValidator<ConfirmEmailRequest>
{
    public ConfirmEmailRequestValidator()
    {
        RuleFor(x => x.Token).NotEmpty().MaximumLength(128);
    }
}
