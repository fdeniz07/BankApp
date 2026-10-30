using Application.Modules.Identity.Features.Auth.Rules;
using Application.Modules.Identity.Features.StaffUsers.Constants;
using Application.Services.Auth;
using Application.Services.Logging;
using Application.Services.Repositories;
using Core.CrossCuttingConcerns.Exceptions.Types;
using Core.Security.Constants;
using Core.Security.Entities;
using Core.Security.Hashing;
using Domain.Entities;
using MediatR;

namespace Application.Modules.Identity.Features.StaffUsers.Commands.Create;

public class CreateStaffUserRequestHandler
    : IRequestHandler<CreateStaffUserRequest, CreatedStaffUserResponse>
{
    private readonly IApplicationUserRepository _applicationUserRepository;
    private readonly IOperationClaimRepository _operationClaimRepository;
    private readonly IUserOperationClaimRepository _userOperationClaimRepository;
    private readonly IBranchRepository _branchRepository;
    private readonly AuthBusinessRules _authBusinessRules;
    private readonly ICurrentUserService _currentUserService;
    private readonly IBranchAuthorizationService _branchAuthorization;
    private readonly IAuditLogger _auditLogger;

    public CreateStaffUserRequestHandler(
        IApplicationUserRepository applicationUserRepository,
        IOperationClaimRepository operationClaimRepository,
        IUserOperationClaimRepository userOperationClaimRepository,
        IBranchRepository branchRepository,
        AuthBusinessRules authBusinessRules,
        ICurrentUserService currentUserService,
        IBranchAuthorizationService branchAuthorization,
        IAuditLogger auditLogger)
    {
        _applicationUserRepository = applicationUserRepository;
        _operationClaimRepository = operationClaimRepository;
        _userOperationClaimRepository = userOperationClaimRepository;
        _branchRepository = branchRepository;
        _authBusinessRules = authBusinessRules;
        _currentUserService = currentUserService;
        _branchAuthorization = branchAuthorization;
        _auditLogger = auditLogger;
    }

    public async Task<CreatedStaffUserResponse> Handle(
        CreateStaffUserRequest request,
        CancellationToken cancellationToken)
    {
        string email = request.Email.Trim();
        await _authBusinessRules.EmailShouldBeUnique(email);

        Guid? branchId = ResolveBranchIdForCreate(request.BranchId);
        Branch? assignedBranch = null;
        if (branchId.HasValue)
        {
            assignedBranch = await _branchRepository.GetAsync(
                b => b.Id == branchId.Value && b.IsActive,
                cancellationToken: cancellationToken);
            if (assignedBranch is null)
                throw new BusinessException(StaffUserMessages.BranchNotFoundOrInactive);
        }

        ApplicationUser provisional = new() { BranchId = branchId };
        await _branchAuthorization.EnsureCanManageStaffUserAsync(provisional, cancellationToken);

        OperationClaim? staffClaim = await _operationClaimRepository.GetAsync(
            c => c.Name == GeneralOperationClaims.BankStaff,
            cancellationToken: cancellationToken);
        if (staffClaim is null)
            throw new BusinessException(StaffUserMessages.BankStaffClaimMissing);

        OperationClaim? managerClaim = null;
        if (request.IsManager)
        {
            managerClaim = await _operationClaimRepository.GetAsync(
                c => c.Name == GeneralOperationClaims.BankManager,
                cancellationToken: cancellationToken);
            if (managerClaim is null)
                throw new BusinessException(StaffUserMessages.BankManagerClaimMissing);
        }

        HashingHelper.CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

        ApplicationUser user = new()
        {
            Email = email,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt,
            CustomerId = null,
            BranchId = branchId,
            IsActive = true,
            FirstName = TrimOrNull(request.FirstName),
            LastName = TrimOrNull(request.LastName),
            Title = request.Title,
            Gender = request.Gender,
            DateOfBirth = request.DateOfBirth,
            Position = request.Position
        };
        await _applicationUserRepository.AddAsync(user, cancellationToken);

        List<string> roles = [GeneralOperationClaims.BankStaff];
        await _userOperationClaimRepository.AddAsync(
            new UserOperationClaim
            {
                UserId = user.Id,
                OperationClaimId = staffClaim.Id
            },
            cancellationToken);

        if (managerClaim is not null)
        {
            await _userOperationClaimRepository.AddAsync(
                new UserOperationClaim
                {
                    UserId = user.Id,
                    OperationClaimId = managerClaim.Id
                },
                cancellationToken);
            roles.Add(GeneralOperationClaims.BankManager);
        }

        _auditLogger.Log(
            AuditEventCategories.Staff,
            AuditEventActions.Create,
            "Staff user created.",
            actorUserId: _currentUserService.UserId,
            actorEmail: null,
            targetType: nameof(ApplicationUser),
            targetId: user.Id,
            changes: new { user.Email, request.IsManager, user.FirstName, user.LastName, user.Position, user.BranchId });

        return new CreatedStaffUserResponse
        {
            Id = user.Id,
            Email = user.Email,
            IsActive = user.IsActive,
            IsManager = request.IsManager,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Title = user.Title,
            Gender = user.Gender,
            DateOfBirth = user.DateOfBirth,
            Position = user.Position,
            BranchId = user.BranchId,
            BranchCode = assignedBranch?.Code,
            BranchName = assignedBranch?.Name,
            ProfileImageUrl = user.ProfileImagePath,
            Roles = roles
        };
    }

    private Guid? ResolveBranchIdForCreate(Guid? requestedBranchId)
    {
        if (_currentUserService.IsSuperAdmin)
            return requestedBranchId;

        return _currentUserService.BranchId;
    }

    private static string? TrimOrNull(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
