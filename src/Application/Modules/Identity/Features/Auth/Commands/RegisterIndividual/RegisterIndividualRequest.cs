using MediatR;

namespace Application.Modules.Identity.Features.Auth.Commands.RegisterIndividual;

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
    /// <summary>UI locale: tr | en | de — used in customer number prefix.</summary>
    public string? Locale { get; set; }
}
