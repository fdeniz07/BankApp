using Application.Modules.Credit.Features.CreditTypes.Queries.GetList;
using Core.Application.Responses;
using Domain.Entities;

namespace Application.Services.Search;

public sealed class NullCreditTypeSearchService : ICreditTypeSearchService
{
    public bool IsAvailable => false;

    public Task EnsureIndexAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task IndexAsync(CreditTypeSearchDocument document, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task UpsertFromCreditTypeAsync(CreditType creditType, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task DeleteAsync(Guid creditTypeId, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task ReindexAllAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task<GetListResponse<GetListCreditTypeListItemDto>?> SearchAsync(
        CreditTypeSearchQuery query,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<GetListResponse<GetListCreditTypeListItemDto>?>(null);

    public Task<CreditTypeFacetsResponse?> GetFacetsAsync(
        CreditTypeSearchQuery query,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<CreditTypeFacetsResponse?>(null);
}
