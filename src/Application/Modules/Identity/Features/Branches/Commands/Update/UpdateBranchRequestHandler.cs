using Application.Services.Auth;
using Application.Services.Logging;
using Application.Services.Repositories;
using Core.CrossCuttingConcerns.Exceptions.Types;
using Domain.Entities;
using MediatR;

namespace Application.Modules.Identity.Features.Branches.Commands.Update;

public class UpdateBranchRequestHandler : IRequestHandler<UpdateBranchRequest, UpdatedBranchResponse>
{
    private readonly IBranchRepository _branchRepository;
    private readonly IBranchAuthorizationService _branchAuthorization;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditLogger _auditLogger;

    public UpdateBranchRequestHandler(
        IBranchRepository branchRepository,
        IBranchAuthorizationService branchAuthorization,
        ICurrentUserService currentUser,
        IAuditLogger auditLogger)
    {
        _branchRepository = branchRepository;
        _branchAuthorization = branchAuthorization;
        _currentUser = currentUser;
        _auditLogger = auditLogger;
    }

    public async Task<UpdatedBranchResponse> Handle(UpdateBranchRequest request, CancellationToken cancellationToken)
    {
        _branchAuthorization.EnsureCanManageBranches();

        Branch? branch = await _branchRepository.GetAsync(
            b => b.Id == request.Id,
            cancellationToken: cancellationToken);
        if (branch is null)
            throw new BusinessException("Branch not found.");

        string code = request.Code.Trim().ToUpperInvariant();
        Branch? duplicate = await _branchRepository.GetAsync(
            b => b.Code == code && b.Id != request.Id && !b.IsDeleted,
            cancellationToken: cancellationToken);
        if (duplicate is not null)
            throw new BusinessException($"Branch code '{code}' already exists.");

        branch.Code = code;
        branch.Name = request.Name.Trim();
        branch.City = string.IsNullOrWhiteSpace(request.City) ? null : request.City.Trim();
        branch.IsActive = request.IsActive;

        await _branchRepository.UpdateAsync(branch, cancellationToken);

        _auditLogger.Log(
            AuditEventCategories.Staff,
            AuditEventActions.Update,
            "Branch updated.",
            actorUserId: _currentUser.UserId,
            targetType: nameof(Branch),
            targetId: branch.Id,
            changes: new { branch.Code, branch.Name, branch.City, branch.IsActive });

        return new UpdatedBranchResponse
        {
            Id = branch.Id,
            Code = branch.Code,
            Name = branch.Name,
            City = branch.City,
            IsActive = branch.IsActive
        };
    }
}
