using Core.Security.JWT;

namespace Application.Modules.Identity.Features.Auth.Commands.Register;

public class RegisteredResponse
{
    public AccessToken AccessToken { get; set; } = default!;
}
