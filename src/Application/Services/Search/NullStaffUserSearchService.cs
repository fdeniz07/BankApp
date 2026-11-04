using Application.Modules.Identity.Features.StaffUsers.Queries.GetList;
using Core.Application.Responses;
using Domain.Entities;

namespace Application.Services.Search;

/// <summary>No-op search when Elasticsearch is disabled (SQL list remains).</summary>
public sealed class NullStaffUserSearchService : IStaffUserSearchService
{
    public bool IsAvailable => false;

    public Task EnsureIndexAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task IndexAsync(StaffUserSearchDocument document, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task UpsertFromUserAsync(ApplicationUser user, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task ReindexAllAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task<GetListResponse<GetListStaffUserListItemDto>?> SearchAsync(
        StaffUserSearchQuery query,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<GetListResponse<GetListStaffUserListItemDto>?>(null);
}
