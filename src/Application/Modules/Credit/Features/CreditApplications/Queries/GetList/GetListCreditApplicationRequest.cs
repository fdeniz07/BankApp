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
    /// <summary>Live search text — prefers Elasticsearch when enabled; SQL fallback.</summary>
    public string? SearchText { get; set; }
    /// <summary>Filter by product currency (Manager facet).</summary>
    public string? CurrencyCode { get; set; }
    /// <summary>Inclusive lower bound for RequestedAmount (Manager facet / advanced filter).</summary>
    public decimal? AmountMin { get; set; }
    /// <summary>Exclusive upper bound for RequestedAmount.</summary>
    public decimal? AmountMax { get; set; }
    public string? SortBy { get; set; }
    public bool SortDesc { get; set; }
}
