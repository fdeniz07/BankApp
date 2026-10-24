using MediatR;

namespace Application.Modules.Identity.Features.Auth.Commands.ConfirmEmail;

public class ConfirmEmailRequest : IRequest<ConfirmEmailResponse>
{
    public string Token { get; set; } = default!;
}
