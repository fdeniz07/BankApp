using Application.Services.Auth;
using Application.Services.Repositories;
using Core.CrossCuttingConcerns.Exceptions.Types;
using Core.Security.Constants;
using Domain.Entities;
using MediatR;

namespace Application.Features.Profiles.Queries.GetMine;

public class GetMyProfileRequestHandler : IRequestHandler<GetMyProfileRequest, GetMyProfileResponse>
{
    private readonly IApplicationUserRepository _applicationUserRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetMyProfileRequestHandler(
        IApplicationUserRepository applicationUserRepository,
        ICurrentUserService currentUserService)
    {
        _applicationUserRepository = applicationUserRepository;
        _currentUserService = currentUserService;
    }

    public async Task<GetMyProfileResponse> Handle(
        GetMyProfileRequest request,
        CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
            throw new AuthorizationException("You are not authenticated.");

        ApplicationUser? user = await _applicationUserRepository.GetAsync(
            u => u.Id == _currentUserService.UserId.Value,
            cancellationToken: cancellationToken);
        if (user is null)
            throw new AuthorizationException("User not found.");

        var claims = await _applicationUserRepository.GetClaimsAsync(user, cancellationToken);
        List<string> roles = claims.Select(c => c.Name).Distinct().OrderBy(n => n).ToList();

        return new GetMyProfileResponse
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Title = user.Title,
            Gender = user.Gender,
            DateOfBirth = user.DateOfBirth,
            Position = user.Position,
            ProfileImageUrl = user.ProfileImagePath,
            IsActive = user.IsActive,
            IsStaff = !user.CustomerId.HasValue,
            Roles = roles
        };
    }
}
