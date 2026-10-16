using Core.Security.JWT;

namespace Application.Features.Auth.Commands.RegisterIndividual;

public class RegisteredIndividualResponse
{
    public AccessToken AccessToken { get; set; } = default!;
    public string? DisplayName { get; set; }
    public string? CustomerCategory { get; set; }
}
