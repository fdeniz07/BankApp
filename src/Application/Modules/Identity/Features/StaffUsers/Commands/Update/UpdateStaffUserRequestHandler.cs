using Application.Modules.Identity.Features.Auth.Rules;
using Application.Modules.Identity.Features.StaffUsers.Constants;
using Application.Modules.Identity.Features.StaffUsers.Rules;
using Application.Services.Auth;
using Application.Services.Logging;
using Application.Services.Repositories;
using Core.CrossCuttingConcerns.Exceptions.Types;
using Core.Security.Constants;
using Core.Security.Entities;
using Core.Security.Hashing;
using Domain.Entities;
using MediatR;

namespace Application.Modules.Identity.Features.StaffUsers.Commands.Update;

public class UpdateStaffUserRequestHandler
    : IRequestHandler<UpdateStaffUserRequest, UpdatedStaffUserResponse>
{
    private readonly IApplicationUserRepository _applicationUserRepository;
    private readonly IOperationClaimRepository _operationClaimRepository;
    private readonly IUserOperationClaimRepository _userOperationClaimRepository;
    private readonly StaffUserBusinessRules _businessRules;
    private readonly AuthBusinessRules _authBusinessRules;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditLogger _auditLogger;

    public UpdateStaffUserRequestHandler(
        IApplicationUserRepository applicationUserRepository,
        IOperationClaimRepository operationClaimRepository,
        IUserOperationClaimRepository userOperationClaimRepository,
        StaffUserBusinessRules businessRules,
        AuthBusinessRules authBusinessRules,
        ICurrentUserService currentUserService,
        IAuditLogger auditLogger)
    {
        _applicationUserRepository = applicationUserRepository;
        _operationClaimRepository = operationClaimRepository;
        _userOperationClaimRepository = userOperationClaimRepository;
        _businessRules = businessRules;
        _authBusinessRules = authBusinessRules;
        _currentUserService = currentUserService;
        _auditLogger = auditLogger;
    }

    public async Task<UpdatedStaffUserResponse> Handle(
        UpdateStaffUserRequest request,
        CancellationToken cancellationToken)
    {
        ApplicationUser? user = await _applicationUserRepository.GetAsync(
            u => u.Id == request.Id,
            cancellationToken: cancellationToken);

        await _businessRules.StaffUserShouldExist(user);
        await _businessRules.UserShouldBeStaffAccount(user!);

        bool isSelf = _currentUserService.UserId.HasValue && _currentUserService.UserId.Value == user!.Id;
        bool isManager = _currentUserService.IsBankManager;

        if (!isManager && !isSelf)
            throw new AuthorizationException("You can only update your own staff profile.");

        bool currentIsManager = await _businessRules.UserHasManagerRoleAsync(user.Id);
        bool nextIsActive = user.IsActive;
        bool nextIsManager = currentIsManager;

        if (isManager)
        {
            nextIsActive = request.IsActive;
            nextIsManager = request.IsManager;
            await _businessRules.CannotDeactivateSelf(user.Id, _currentUserService.UserId, nextIsActive);
            await _businessRules.CannotDeactivateLastActiveManager(user, nextIsActive);
            await _businessRules.CannotDemoteLastActiveManager(user, nextIsManager);
        }

        string email = request.Email.Trim();
        await _authBusinessRules.EmailShouldBeUnique(email, user.Id);

        var changes = new Dictionary<string, object?>();
        if (!string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase))
            changes["Email"] = new { from = user.Email, to = email };
        if (user.FirstName != TrimOrNull(request.FirstName))
            changes["FirstName"] = new { from = user.FirstName, to = TrimOrNull(request.FirstName) };
        if (user.LastName != TrimOrNull(request.LastName))
            changes["LastName"] = new { from = user.LastName, to = TrimOrNull(request.LastName) };
        if (user.Title?.Name != request.Title?.Name)
            changes["Title"] = new { from = user.Title?.Name, to = request.Title?.Name };
        if (user.Gender?.Name != request.Gender?.Name)
            changes["Gender"] = new { from = user.Gender?.Name, to = request.Gender?.Name };
        if (user.DateOfBirth != request.DateOfBirth)
            changes["DateOfBirth"] = new { from = user.DateOfBirth, to = request.DateOfBirth };
        if (user.Position?.Name != request.Position?.Name)
            changes["Position"] = new { from = user.Position?.Name, to = request.Position?.Name };
        if (isManager && user.IsActive != nextIsActive)
            changes["IsActive"] = new { from = user.IsActive, to = nextIsActive };
        if (isManager && currentIsManager != nextIsManager)
            changes["IsManager"] = new { from = currentIsManager, to = nextIsManager };
        if (!string.IsNullOrWhiteSpace(request.NewPassword))
            changes["Password"] = new { changed = true };

        user.Email = email;
        user.FirstName = TrimOrNull(request.FirstName);
        user.LastName = TrimOrNull(request.LastName);
        user.Title = request.Title;
        user.Gender = request.Gender;
        user.DateOfBirth = request.DateOfBirth;
        user.Position = request.Position;
        user.IsActive = nextIsActive;

        if (!string.IsNullOrWhiteSpace(request.NewPassword))
        {
            HashingHelper.CreatePasswordHash(request.NewPassword, out byte[] hash, out byte[] salt);
            user.PasswordHash = hash;
            user.PasswordSalt = salt;
        }

        await _applicationUserRepository.UpdateAsync(user, cancellationToken);

        if (isManager)
            await SyncManagerRoleAsync(user.Id, nextIsManager, cancellationToken);

        List<OperationClaim> claims = await _applicationUserRepository.GetClaimsAsync(user, cancellationToken);
        List<string> roles = claims.Select(c => c.Name).Distinct().OrderBy(n => n).ToList();

        _auditLogger.Log(
            AuditEventCategories.Staff,
            AuditEventActions.Update,
            "Staff user updated.",
            actorUserId: _currentUserService.UserId,
            targetType: nameof(ApplicationUser),
            targetId: user.Id,
            changes: changes.Count == 0 ? null : changes);

        return new UpdatedStaffUserResponse
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
            Roles = roles
        };
    }

    private async Task SyncManagerRoleAsync(Guid userId, bool isManager, CancellationToken cancellationToken)
    {
        OperationClaim? managerClaim = await _operationClaimRepository.GetAsync(
            c => c.Name == GeneralOperationClaims.BankManager,
            cancellationToken: cancellationToken);
        if (managerClaim is null)
            throw new BusinessException(StaffUserMessages.BankManagerClaimMissing);

        UserOperationClaim? existing = await _userOperationClaimRepository.GetAsync(
            uoc => uoc.UserId == userId && uoc.OperationClaimId == managerClaim.Id,
            withDeleted: true,
            cancellationToken: cancellationToken);

        if (isManager)
        {
            if (existing is null)
            {
                await _userOperationClaimRepository.AddAsync(
                    new UserOperationClaim
                    {
                        UserId = userId,
                        OperationClaimId = managerClaim.Id
                    },
                    cancellationToken);
            }
            else if (existing.IsDeleted)
            {
                existing.IsDeleted = false;
                existing.DeletedDate = null;
                await _userOperationClaimRepository.UpdateAsync(existing, cancellationToken);
            }
        }
        else if (existing is not null && !existing.IsDeleted)
        {
            await _userOperationClaimRepository.DeleteAsync(existing, cancellationToken: cancellationToken);
        }
    }

    private static string? TrimOrNull(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
