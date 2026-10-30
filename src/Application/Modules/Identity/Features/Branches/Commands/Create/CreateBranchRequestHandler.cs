using Application.Services.Auth;
using Application.Services.Logging;
using Application.Services.Repositories;
using Core.CrossCuttingConcerns.Exceptions.Types;
using Domain.Entities;
using MediatR;

namespace Application.Modules.Identity.Features.Branches.Commands.Create;

public class CreateBranchRequestHandler : IRequestHandler<CreateBranchRequest, CreatedBranchResponse>
{
    private readonly IBranchRepository _branchRepository;
    private readonly IBranchAuthorizationService _branchAuthorization;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditLogger _auditLogger;

    public CreateBranchRequestHandler(
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

    public async Task<CreatedBranchResponse> Handle(CreateBranchRequest request, CancellationToken cancellationToken)
    {
        _branchAuthorization.EnsureCanManageBranches();

        string code = request.Code.Trim().ToUpperInvariant();
        Branch? existing = await _branchRepository.GetAsync(
            b => b.Code == code && !b.IsDeleted,
            cancellationToken: cancellationToken);
        if (existing is not null)
            throw new BusinessException($"Branch code '{code}' already exists.");

        Branch branch = new()
        {
            Code = code,
            Name = request.Name.Trim(),
            City = string.IsNullOrWhiteSpace(request.City) ? null : request.City.Trim(),
            IsActive = request.IsActive
        };
        await _branchRepository.AddAsync(branch, cancellationToken);

        _auditLogger.Log(
            AuditEventCategories.Staff,
            AuditEventActions.Create,
            "Branch created.",
            actorUserId: _currentUser.UserId,
            targetType: nameof(Branch),
            targetId: branch.Id,
            changes: new { branch.Code, branch.Name, branch.City });

        return new CreatedBranchResponse
        {
            Id = branch.Id,
            Code = branch.Code,
            Name = branch.Name,
            City = branch.City,
            IsActive = branch.IsActive
        };
    }
}
