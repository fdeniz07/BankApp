namespace Application.Modules.Identity.Features.Auth.Commands.ConfirmEmail;

public class ConfirmEmailResponse
{
    public string Email { get; set; } = default!;
    public bool Confirmed { get; set; }
}
