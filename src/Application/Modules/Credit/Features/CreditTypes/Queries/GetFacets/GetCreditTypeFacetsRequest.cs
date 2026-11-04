using Application.Services.Search;
using Core.Application.Pipelines.Authorization;
using Core.Security.Constants;
using MediatR;

namespace Application.Modules.Credit.Features.CreditTypes.Queries.GetFacets;

public class GetCreditTypeFacetsRequest : IRequest<CreditTypeFacetsResponse>, ISecuredRequest
{
    public string[] Roles =>
        [GeneralOperationClaims.BankManager, GeneralOperationClaims.SuperAdmin];

    public string? Search { get; set; }
    public string? Locale { get; set; }
    public string? Category { get; set; }
    public string? CurrencyCode { get; set; }
    public string? RiskLevel { get; set; }
    public bool? IsActive { get; set; }
}
