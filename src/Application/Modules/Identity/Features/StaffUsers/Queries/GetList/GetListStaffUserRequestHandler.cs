using System.Linq.Expressions;
using Application.Services.Auth;
using Application.Services.Repositories;
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

    public GetListStaffUserRequestHandler(
        IApplicationUserRepository applicationUserRepository,
        IBranchAuthorizationService branchAuthorization)
    {
        _applicationUserRepository = applicationUserRepository;
        _branchAuthorization = branchAuthorization;
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

        Expression<Func<ApplicationUser, bool>> predicate = u =>
            u.CustomerId == null
            && (!request.IsActive.HasValue || u.IsActive == request.IsActive.Value)
            && (!branchFilter.HasValue || u.BranchId == branchFilter.Value);

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
