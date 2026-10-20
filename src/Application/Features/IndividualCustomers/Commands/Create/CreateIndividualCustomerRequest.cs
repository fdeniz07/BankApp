using Core.Application.Pipelines.Authorization;
using Core.Security.Constants;
using MediatR;

namespace Application.Features.IndividualCustomers.Commands.Create;

public class CreateIndividualCustomerRequest : IRequest<CreatedIndividualCustomerResponse>, ISecuredRequest
{
    public string[] Roles => [GeneralOperationClaims.BankStaff, GeneralOperationClaims.BankManager];
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string PhoneNumber { get; set; } = default!;
    public string Address { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string NationalId { get; set; } = default!;
    public DateOnly? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? MotherName { get; set; }
    public string? FatherName { get; set; }
    /// <summary>UI locale: tr | en | de — used in generated customer number.</summary>
    public string? Locale { get; set; }
}
