using Application.Modules.Credit.Features.CreditApplications.Queries.GetFacets;
using Application.Modules.Credit.Features.CreditApplications.Queries.GetList;
using Core.Application.Responses;
using Domain.Entities;

namespace Application.Services.Search;

public interface ICreditApplicationSearchService
{
    bool IsAvailable { get; }

    Task EnsureIndexAsync(CancellationToken cancellationToken = default);

    Task IndexAsync(CreditApplicationSearchDocument document, CancellationToken cancellationToken = default);

    Task UpsertFromApplicationAsync(CreditApplication application, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid applicationId, CancellationToken cancellationToken = default);

    Task ReindexAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Full-text and/or filter search. Returns null when ES is unavailable or the query should fall back to SQL.
    /// Text may be empty when amount/status/branch filters drive the query.
    /// </summary>
    Task<GetListResponse<GetListCreditApplicationListItemDto>?> SearchAsync(
        CreditApplicationSearchQuery query,
        CancellationToken cancellationToken = default);

    /// <summary>Aggregation facets. Returns null when ES is unavailable (caller uses SQL).</summary>
    Task<CreditApplicationFacetsResponse?> GetFacetsAsync(
        CreditApplicationSearchQuery query,
        CancellationToken cancellationToken = default);
}

public sealed class CreditApplicationSearchQuery
{
    public string Text { get; set; } = "";
    public int PageIndex { get; set; }
    public int PageSize { get; set; } = 10;
    public string? Status { get; set; }
    public Guid? BranchId { get; set; }
    public bool? MissingRequiredDocumentsOnly { get; set; }
    public string? CurrencyCode { get; set; }
    public decimal? AmountMin { get; set; }
    public decimal? AmountMax { get; set; }
    public string? SortBy { get; set; }
    public bool SortDesc { get; set; } = true;

    /// <summary>Staff: forced branch scope. Null = no extra filter (SuperAdmin all).</summary>
    public Guid? RequiredBranchScope { get; set; }

    public bool AllowCrossBranchFilters { get; set; }
}
