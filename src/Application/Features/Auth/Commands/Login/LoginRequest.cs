using MediatR;

namespace Application.Features.Auth.Commands.Login;

public class LoginRequest : IRequest<LoggedResponse>
{
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
    public bool RememberMe { get; set; }
}
