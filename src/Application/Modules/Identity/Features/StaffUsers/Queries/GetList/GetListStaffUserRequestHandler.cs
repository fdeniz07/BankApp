using System.Linq.Expressions;
using Application.Services.Auth;
using Application.Services.Repositories;
using Application.Services.Search;
using Core.Application.Responses;
using Core.Repositories;
using Core.Security.Constants;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Modules.Identity.Features.StaffUsers.Queries.GetList;

public class GetListStaffUserRequestHandler
    : IRequestHandler<GetListStaffUserRequest, GetListResponse<GetListStaffUserListItemDto>>
{
    private readonly IApplicationUserRepository _applicationUserRepository;
    private readonly IBranchAuthorizationService _branchAuthorization;
    private readonly ICurrentUserService _currentUser;
    private readonly IStaffUserSearchService _search;

    public GetListStaffUserRequestHandler(
        IApplicationUserRepository applicationUserRepository,
        IBranchAuthorizationService branchAuthorization,
        ICurrentUserService currentUser,
        IStaffUserSearchService search)
    {
        _applicationUserRepository = applicationUserRepository;
        _branchAuthorization = branchAuthorization;
        _currentUser = currentUser;
        _search = search;
    }

    public async Task<GetListResponse<GetListStaffUserListItemDto>> Handle(
        GetListStaffUserRequest request,
        CancellationToken cancellationToken)
    {
        if (_branchAuthorization.MustSeeNoBranchData)
        {
            return new GetListResponse<GetListStaffUserListItemDto>
            {
                Items = [],
                Index = request.PageRequest.PageIndex,
                Size = request.PageRequest.PageSize,
                Count = 0,
                Pages = 0,
                From = 0
            };
        }

        Guid? branchFilter = _branchAuthorization.ResolveListBranchFilter(request.BranchId);

        bool preferSearch = _search.IsAvailable
            && (!string.IsNullOrWhiteSpace(request.Search)
                || request.BranchId.HasValue
                || request.IsManager.HasValue);

        if (preferSearch)
        {
            GetListResponse<GetListStaffUserListItemDto>? es = await _search.SearchAsync(
                new StaffUserSearchQuery
                {
                    Text = request.Search ?? "",
                    PageIndex = request.PageRequest.PageIndex,
                    PageSize = request.PageRequest.PageSize,
                    IsActive = request.IsActive,
                    IsManager = request.IsManager,
                    BranchId = request.BranchId,
                    RequiredBranchScope = _currentUser.IsSuperAdmin ? null : branchFilter,
                    AllowCrossBranchFilters = _currentUser.IsSuperAdmin || _currentUser.IsBankManager
                },
                cancellationToken);
            if (es is not null)
                return es;
        }

        string? search = string.IsNullOrWhiteSpace(request.Search) ? null : request.Search.Trim();
        bool? wantManager = request.IsManager;
        string managerRole = GeneralOperationClaims.BankManager;
        string superAdminRole = GeneralOperationClaims.SuperAdmin;

        Expression<Func<ApplicationUser, bool>> predicate = u =>
            u.CustomerId == null
            && (!request.IsActive.HasValue || u.IsActive == request.IsActive.Value)
            && (!branchFilter.HasValue || u.BranchId == branchFilter.Value)
            && (search == null
                || (u.Email != null && u.Email.Contains(search))
                || (u.FirstName != null && u.FirstName.Contains(search))
                || (u.LastName != null && u.LastName.Contains(search)))
            && (!wantManager.HasValue
                || (wantManager.Value
                    ? u.UserOperationClaims.Any(uoc =>
                        !uoc.IsDeleted
                        && uoc.OperationClaim != null
                        && !uoc.OperationClaim.IsDeleted
                        && (uoc.OperationClaim.Name == managerRole
                            || uoc.OperationClaim.Name == superAdminRole))
                    : !u.UserOperationClaims.Any(uoc =>
                        !uoc.IsDeleted
                        && uoc.OperationClaim != null
                        && !uoc.OperationClaim.IsDeleted
                        && (uoc.OperationClaim.Name == managerRole
                            || uoc.OperationClaim.Name == superAdminRole))));

        IPaginate<ApplicationUser> users = await _applicationUserRepository.GetListAsync(
            predicate: predicate,
            orderBy: q => q.OrderBy(u => u.Email),
            include: q => q
                .Include(u => u.Branch)
                .Include(u => u.UserOperationClaims)
                .ThenInclude(uoc => uoc.OperationClaim),
            index: request.PageRequest.PageIndex,
            size: request.PageRequest.PageSize,
            cancellationToken: cancellationToken);

        return new GetListResponse<GetListStaffUserListItemDto>
        {
            From = users.From,
            Index = users.Index,
            Size = users.Size,
            Count = users.Count,
            Pages = users.Pages,
            Items = users.Items.Select(MapItem).ToList()
        };
    }

    private static GetListStaffUserListItemDto MapItem(ApplicationUser user)
    {
        List<string> roles = user.UserOperationClaims
            .Where(uoc => !uoc.IsDeleted && uoc.OperationClaim is not null && !uoc.OperationClaim.IsDeleted)
            .Select(uoc => uoc.OperationClaim.Name)
            .Distinct()
            .OrderBy(n => n)
            .ToList();

        return new GetListStaffUserListItemDto
        {
            Id = user.Id,
            Email = user.Email,
            IsActive = user.IsActive,
            IsManager = roles.Contains(GeneralOperationClaims.BankManager)
                || roles.Contains(GeneralOperationClaims.SuperAdmin),
            FirstName = user.FirstName,
            LastName = user.LastName,
            Title = user.Title,
            Gender = user.Gender,
            DateOfBirth = user.DateOfBirth,
            Position = user.Position,
            BranchId = user.BranchId,
            BranchCode = user.Branch?.Code,
            BranchName = user.Branch?.Name,
            ProfileImageUrl = user.ProfileImagePath,
            Roles = roles,
            CreatedDate = user.CreatedDate
        };
    }
}
