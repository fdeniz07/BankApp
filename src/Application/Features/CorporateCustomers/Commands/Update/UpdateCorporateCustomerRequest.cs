using Core.Application.Pipelines.Authorization;
using Core.Security.Constants;
using MediatR;

namespace Application.Features.CorporateCustomers.Commands.Update;

public class UpdateCorporateCustomerRequest : IRequest<UpdatedCorporateCustomerResponse>, ISecuredRequest
{
    public string[] Roles =>
        [GeneralOperationClaims.Customer, GeneralOperationClaims.BankStaff, GeneralOperationClaims.BankManager];
    public Guid Id { get; set; }
    public string CustomerNumber { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string PhoneNumber { get; set; } = default!;
    public string Address { get; set; } = default!;
    public bool IsActive { get; set; }
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
}
