namespace Application.Features.Auth.Commands.RegisterIndividual;

public class RegisteredIndividualResponse
{
    public bool RequiresEmailConfirmation { get; set; }
    public string Email { get; set; } = default!;
    public string? DisplayName { get; set; }
    public string? CustomerCategory { get; set; }
    public string Message { get; set; } = default!;
}
