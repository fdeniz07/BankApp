using MediatR;

namespace Application.Features.Auth.Commands.ConfirmEmail;

public class ConfirmEmailRequest : IRequest<ConfirmEmailResponse>
{
    public string Token { get; set; } = default!;
}
