using Core.Application.Pipelines.Authorization;
using Core.Security.Constants;
using MediatR;

namespace Application.Features.CorporateCustomers.Commands.Create;

public class CreateCorporateCustomerRequest : IRequest<CreatedCorporateCustomerResponse>, ISecuredRequest
{
    public string[] Roles => [GeneralOperationClaims.BankStaff, GeneralOperationClaims.BankManager];
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string PhoneNumber { get; set; } = default!;
    public string Address { get; set; } = default!;
    public string CompanyName { get; set; } = default!;
    public string TaxNumber { get; set; } = default!;
    public string TaxOffice { get; set; } = default!;
    public string CompanyRegistrationNumber { get; set; } = default!;
    public string? CompanyEmail { get; set; }
    public string AuthorizedPersonName { get; set; } = default!;
    public string? CompanyAddress { get; set; }
    public string? CompanyType { get; set; }
    public string? CompanySector { get; set; }
    public string? CompanyIndustry { get; set; }
    public string? CompanySubIndustry { get; set; }
    public DateTime? CompanyFoundationDate { get; set; }
    /// <summary>UI locale: tr | en | de — used in generated customer number.</summary>
    public string? Locale { get; set; }
}
