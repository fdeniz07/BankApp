using System.Linq.Expressions;
using Application.Services.Repositories;
using Core.Application.Responses;
using Core.Repositories;
using Core.Security.Constants;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.StaffUsers.Queries.GetList;

public class GetListStaffUserRequestHandler
    : IRequestHandler<GetListStaffUserRequest, GetListResponse<GetListStaffUserListItemDto>>
{
    private readonly IApplicationUserRepository _applicationUserRepository;

    public GetListStaffUserRequestHandler(IApplicationUserRepository applicationUserRepository)
    {
        _applicationUserRepository = applicationUserRepository;
    }

    public async Task<GetListResponse<GetListStaffUserListItemDto>> Handle(
        GetListStaffUserRequest request,
        CancellationToken cancellationToken)
    {
        Expression<Func<ApplicationUser, bool>> predicate = u => u.CustomerId == null;
        if (request.IsActive.HasValue)
        {
            bool isActive = request.IsActive.Value;
            predicate = u => u.CustomerId == null && u.IsActive == isActive;
        }

        IPaginate<ApplicationUser> users = await _applicationUserRepository.GetListAsync(
            predicate: predicate,
            orderBy: q => q.OrderBy(u => u.Email),
            include: q => q
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
            IsManager = roles.Contains(GeneralOperationClaims.BankManager),
            FirstName = user.FirstName,
            LastName = user.LastName,
            Title = user.Title,
            Gender = user.Gender,
            DateOfBirth = user.DateOfBirth,
            Position = user.Position,
            ProfileImageUrl = user.ProfileImagePath,
            Roles = roles,
            CreatedDate = user.CreatedDate
        };
    }
}
