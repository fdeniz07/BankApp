using Application.Modules.Credit.Features.CreditApplications.Queries.GetFacets;
using Application.Modules.Credit.Features.CreditTypes.Queries.GetList;
using Core.Application.Responses;
using Domain.Entities;

namespace Application.Services.Search;

public interface ICreditTypeSearchService
{
    bool IsAvailable { get; }

    Task EnsureIndexAsync(CancellationToken cancellationToken = default);

    Task IndexAsync(CreditTypeSearchDocument document, CancellationToken cancellationToken = default);

    Task UpsertFromCreditTypeAsync(CreditType creditType, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid creditTypeId, CancellationToken cancellationToken = default);

    Task ReindexAllAsync(CancellationToken cancellationToken = default);

    Task<GetListResponse<GetListCreditTypeListItemDto>?> SearchAsync(
        CreditTypeSearchQuery query,
        CancellationToken cancellationToken = default);

    Task<CreditTypeFacetsResponse?> GetFacetsAsync(
        CreditTypeSearchQuery query,
        CancellationToken cancellationToken = default);
}

public sealed class CreditTypeSearchQuery
{
    public string Text { get; set; } = "";
    public int PageIndex { get; set; }
    public int PageSize { get; set; } = 10;
    public string? Locale { get; set; }
    public string? Category { get; set; }
    public string? CurrencyCode { get; set; }
    public string? RiskLevel { get; set; }
    public bool? IsActive { get; set; }
    public string? SortBy { get; set; }
    public bool SortDesc { get; set; }
}

public sealed class CreditTypeFacetsResponse
{
    public string Source { get; set; } = "Sql";
    public List<FacetBucketDto> Locales { get; set; } = [];
    public List<FacetBucketDto> Categories { get; set; } = [];
    public List<FacetBucketDto> Currencies { get; set; } = [];
    public List<FacetBucketDto> RiskLevels { get; set; } = [];
}
