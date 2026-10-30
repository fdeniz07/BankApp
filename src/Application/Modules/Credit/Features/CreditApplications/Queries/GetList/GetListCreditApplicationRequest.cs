using Core.Application.Requests;
using Core.Application.Responses;
using Domain.Enums;
using Core.Application.Pipelines.Authorization;
using Core.Security.Constants;
using MediatR;

namespace Application.Modules.Credit.Features.CreditApplications.Queries.GetList;

public class GetListCreditApplicationRequest : IRequest<GetListResponse<GetListCreditApplicationListItemDto>>, ISecuredRequest
{
    public string[] Roles => [GeneralOperationClaims.Customer, GeneralOperationClaims.BankStaff];
    public PageRequest PageRequest { get; set; } = new();
    public Guid? CustomerId { get; set; }
    public Guid? BranchId { get; set; }
    public CreditApplicationStatus? Status { get; set; }
    /// <summary>When true, only Pending applications missing IdentityDocument or IncomeProof.</summary>
    public bool? MissingRequiredDocumentsOnly { get; set; }
    public string? SortBy { get; set; }
    public bool SortDesc { get; set; }
}
