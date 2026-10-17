using Application.Features.StaffUsers.Constants;
using Application.Services.Repositories;
using Core.Application.Pipelines;
using Core.CrossCuttingConcerns.Exceptions.Types;
using Core.Security.Constants;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.StaffUsers.Rules;

public class StaffUserBusinessRules : BaseBusinessRules
{
    private readonly IApplicationUserRepository _applicationUserRepository;
    private readonly IUserOperationClaimRepository _userOperationClaimRepository;
    private readonly IOperationClaimRepository _operationClaimRepository;

    public StaffUserBusinessRules(
        IApplicationUserRepository applicationUserRepository,
        IUserOperationClaimRepository userOperationClaimRepository,
        IOperationClaimRepository operationClaimRepository)
    {
        _applicationUserRepository = applicationUserRepository;
        _userOperationClaimRepository = userOperationClaimRepository;
        _operationClaimRepository = operationClaimRepository;
    }

    public Task StaffUserShouldExist(ApplicationUser? user)
    {
        if (user is null)
            throw new BusinessException(StaffUserMessages.NotExists);

        return Task.CompletedTask;
    }

    public Task UserShouldBeStaffAccount(ApplicationUser user)
    {
        if (user.CustomerId.HasValue)
            throw new BusinessException(StaffUserMessages.NotStaffAccount);

        return Task.CompletedTask;
    }

    public Task CannotDeactivateSelf(Guid targetUserId, Guid? currentUserId, bool willBeActive)
    {
        if (!willBeActive && currentUserId.HasValue && currentUserId.Value == targetUserId)
            throw new BusinessException(StaffUserMessages.CannotModifySelf);

        return Task.CompletedTask;
    }

    public async Task CannotDeactivateLastActiveManager(ApplicationUser user, bool willBeActive)
    {
        if (willBeActive || !user.IsActive)
            return;

        if (!await UserHasManagerRoleAsync(user.Id))
            return;

        int activeManagerCount = await CountActiveManagersAsync();
        if (activeManagerCount <= 1)
            throw new BusinessException(StaffUserMessages.CannotDeactivateLastManager);
    }

    public async Task CannotDemoteLastActiveManager(ApplicationUser user, bool willBeManager)
    {
        if (willBeManager || !user.IsActive)
            return;

        if (!await UserHasManagerRoleAsync(user.Id))
            return;

        int activeManagerCount = await CountActiveManagersAsync();
        if (activeManagerCount <= 1)
            throw new BusinessException(StaffUserMessages.CannotDemoteLastManager);
    }

    public async Task<bool> UserHasManagerRoleAsync(Guid userId)
    {
        Core.Security.Entities.OperationClaim? managerClaim = await _operationClaimRepository.GetAsync(
            c => c.Name == GeneralOperationClaims.BankManager);

        if (managerClaim is null)
            return false;

        return await _userOperationClaimRepository.AnyAsync(
            uoc => uoc.UserId == userId && uoc.OperationClaimId == managerClaim.Id);
    }

    private async Task<int> CountActiveManagersAsync()
    {
        Core.Repositories.IPaginate<ApplicationUser> staffUsers = await _applicationUserRepository.GetListAsync(
            predicate: u => u.CustomerId == null && u.IsActive,
            include: q => q
                .Include(u => u.UserOperationClaims)
                .ThenInclude(uoc => uoc.OperationClaim),
            size: 500);

        return staffUsers.Items.Count(u =>
            u.UserOperationClaims.Any(uoc =>
                !uoc.IsDeleted
                && uoc.OperationClaim is not null
                && !uoc.OperationClaim.IsDeleted
                && uoc.OperationClaim.Name == GeneralOperationClaims.BankManager));
    }
}
