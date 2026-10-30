using Application.Services.Auth;
using Application.Services.Repositories;
using Core.CrossCuttingConcerns.Exceptions.Types;
using Domain.Entities;
using MediatR;

namespace Application.Modules.Identity.Features.Profiles.Queries.GetMine;

public class GetMyProfileRequestHandler : IRequestHandler<GetMyProfileRequest, GetMyProfileResponse>
{
    private readonly IApplicationUserRepository _applicationUserRepository;
    private readonly IIndividualCustomerRepository _individualCustomerRepository;
    private readonly ICorporateCustomerRepository _corporateCustomerRepository;
    private readonly IBranchRepository _branchRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetMyProfileRequestHandler(
        IApplicationUserRepository applicationUserRepository,
        IIndividualCustomerRepository individualCustomerRepository,
        ICorporateCustomerRepository corporateCustomerRepository,
        IBranchRepository branchRepository,
        ICurrentUserService currentUserService)
    {
        _applicationUserRepository = applicationUserRepository;
        _individualCustomerRepository = individualCustomerRepository;
        _corporateCustomerRepository = corporateCustomerRepository;
        _branchRepository = branchRepository;
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

        (string? firstName, string? lastName) = await ResolveNamesAsync(user, cancellationToken);
        Branch? branch = null;
        if (user.BranchId.HasValue)
        {
            branch = await _branchRepository.GetAsync(
                b => b.Id == user.BranchId.Value,
                cancellationToken: cancellationToken);
        }

        return new GetMyProfileResponse
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = firstName,
            LastName = lastName,
            Title = user.Title,
            Gender = user.Gender,
            DateOfBirth = user.DateOfBirth,
            Position = user.Position,
            BranchId = user.BranchId,
            BranchCode = branch?.Code,
            BranchName = branch?.Name,
            ProfileImageUrl = user.ProfileImagePath,
            IsActive = user.IsActive,
            IsStaff = !user.CustomerId.HasValue,
            Roles = roles
        };
    }

    private async Task<(string? FirstName, string? LastName)> ResolveNamesAsync(
        ApplicationUser user,
        CancellationToken cancellationToken)
    {
        string? firstName = user.FirstName;
        string? lastName = user.LastName;
        if (!string.IsNullOrWhiteSpace(firstName) || !string.IsNullOrWhiteSpace(lastName))
            return (firstName, lastName);

        if (!user.CustomerId.HasValue)
            return (firstName, lastName);

        IndividualCustomer? individual = await _individualCustomerRepository.GetAsync(
            c => c.Id == user.CustomerId.Value,
            cancellationToken: cancellationToken);
        if (individual is not null)
            return (individual.FirstName, individual.LastName);

        CorporateCustomer? corporate = await _corporateCustomerRepository.GetAsync(
            c => c.Id == user.CustomerId.Value,
            cancellationToken: cancellationToken);
        if (corporate is not null)
            return (corporate.CompanyName, null);

        return (firstName, lastName);
    }
}
