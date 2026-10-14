using MediatR;

namespace Application.Features.Auth.Commands.Register;

public class RegisterRequest : IRequest<RegisteredResponse>
{
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
    public Guid? CustomerId { get; set; }
}
