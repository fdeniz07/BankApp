using Core.Application.Pipelines.Authorization;
using Core.Security.Constants;
using Domain.Enums;
using MediatR;

namespace Application.Modules.Credit.Features.CreditApplications.Queries.GetFacets;

public sealed class GetCreditApplicationFacetsRequest
    : IRequest<CreditApplicationFacetsResponse>, ISecuredRequest
{
    public string[] Roles => [GeneralOperationClaims.BankManager, GeneralOperationClaims.SuperAdmin];

    public Guid? BranchId { get; set; }
    public CreditApplicationStatus? Status { get; set; }
    public bool? MissingRequiredDocumentsOnly { get; set; }
    public string? SearchText { get; set; }
    public string? CurrencyCode { get; set; }
    public decimal? AmountMin { get; set; }
    public decimal? AmountMax { get; set; }
}
