using Application.Modules.Credit.Features.CreditApplications.Queries.GetFacets;
using Application.Modules.Credit.Features.CreditApplications.Queries.GetList;
using Core.Application.Responses;
using Domain.Entities;

namespace Application.Services.Search;

/// <summary>No-op search when Elasticsearch is disabled (SQL list / facets remain).</summary>
public sealed class NullCreditApplicationSearchService : ICreditApplicationSearchService
{
    public bool IsAvailable => false;

    public Task EnsureIndexAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task IndexAsync(CreditApplicationSearchDocument document, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task UpsertFromApplicationAsync(CreditApplication application, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task DeleteAsync(Guid applicationId, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task ReindexAllAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task<GetListResponse<GetListCreditApplicationListItemDto>?> SearchAsync(
        CreditApplicationSearchQuery query,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<GetListResponse<GetListCreditApplicationListItemDto>?>(null);

    public Task<CreditApplicationFacetsResponse?> GetFacetsAsync(
        CreditApplicationSearchQuery query,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<CreditApplicationFacetsResponse?>(null);
}
