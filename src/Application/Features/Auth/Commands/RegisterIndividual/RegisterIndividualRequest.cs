using MediatR;

namespace Application.Features.Auth.Commands.RegisterIndividual;

public class RegisterIndividualRequest : IRequest<RegisteredIndividualResponse>
{
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string PhoneNumber { get; set; } = default!;
    public string Address { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string NationalId { get; set; } = default!;
    public DateOnly? DateOfBirth { get; set; }
    public string Gender { get; set; } = default!;
    public string? MotherName { get; set; }
    public string? FatherName { get; set; }
}
