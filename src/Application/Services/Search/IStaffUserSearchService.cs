using Application.Modules.Identity.Features.StaffUsers.Queries.GetList;
using Core.Application.Responses;
using Domain.Entities;

namespace Application.Services.Search;

public interface IStaffUserSearchService
{
    bool IsAvailable { get; }

    Task EnsureIndexAsync(CancellationToken cancellationToken = default);

    Task IndexAsync(StaffUserSearchDocument document, CancellationToken cancellationToken = default);

    Task UpsertFromUserAsync(ApplicationUser user, CancellationToken cancellationToken = default);

    Task ReindexAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Full-text and/or filter search. Returns null when ES is unavailable or the query should fall back to SQL.
    /// </summary>
    Task<GetListResponse<GetListStaffUserListItemDto>?> SearchAsync(
        StaffUserSearchQuery query,
        CancellationToken cancellationToken = default);
}

public sealed class StaffUserSearchQuery
{
    public string Text { get; set; } = "";
    public int PageIndex { get; set; }
    public int PageSize { get; set; } = 10;
    public bool? IsActive { get; set; }
    public bool? IsManager { get; set; }
    public Guid? BranchId { get; set; }

    /// <summary>Forced branch scope (non–SuperAdmin). Null = no extra filter.</summary>
    public Guid? RequiredBranchScope { get; set; }

    public bool AllowCrossBranchFilters { get; set; }
}
