using Core.Security.JWT;

namespace Application.Features.Auth.Commands.Login;

public class LoggedResponse
{
    public AccessToken AccessToken { get; set; } = default!;
    public string? DisplayName { get; set; }
    public string? CustomerCategory { get; set; }
}
