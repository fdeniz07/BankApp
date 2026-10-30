using Application.Modules.Credit.Features.CreditApplications.Rules;
using Application.Services.Auth;
using Application.Services.Logging;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;

namespace Application.Modules.Credit.Features.CreditApplications.Commands.Reject;

public class RejectCreditApplicationRequestHandler
    : IRequestHandler<RejectCreditApplicationRequest, RejectedCreditApplicationResponse>
{
    private readonly ICreditApplicationRepository _creditApplicationRepository;
    private readonly IApplicationUserRepository _applicationUserRepository;
    private readonly IBranchRepository _branchRepository;
    private readonly IMapper _mapper;
    private readonly CreditApplicationBusinessRules _businessRules;
    private readonly ICurrentUserService _currentUserService;
    private readonly IBranchAuthorizationService _branchAuthorization;
    private readonly IAuditLogger _auditLogger;

    public RejectCreditApplicationRequestHandler(
        ICreditApplicationRepository creditApplicationRepository,
        IApplicationUserRepository applicationUserRepository,
        IBranchRepository branchRepository,
        IMapper mapper,
        CreditApplicationBusinessRules businessRules,
        ICurrentUserService currentUserService,
        IBranchAuthorizationService branchAuthorization,
        IAuditLogger auditLogger)
    {
        _creditApplicationRepository = creditApplicationRepository;
        _applicationUserRepository = applicationUserRepository;
        _branchRepository = branchRepository;
        _mapper = mapper;
        _businessRules = businessRules;
        _currentUserService = currentUserService;
        _branchAuthorization = branchAuthorization;
        _auditLogger = auditLogger;
    }

    public async Task<RejectedCreditApplicationResponse> Handle(
        RejectCreditApplicationRequest request,
        CancellationToken cancellationToken)
    {
        CreditApplication? creditApplication = await _creditApplicationRepository.GetAsync(
            predicate: c => c.Id == request.Id,
            cancellationToken: cancellationToken);

        await _businessRules.CreditApplicationShouldExistWhenSelected(creditApplication);
        await _branchAuthorization.EnsureCanModifyApplicationAsync(creditApplication!, cancellationToken);
        await _businessRules.ApplicationShouldBePending(creditApplication!);

        (string? decisionBy, string? branchCode, string? branchName) =
            await ResolveDecisionMakerAsync(cancellationToken);
        creditApplication!.Reject(
            request.RejectionReason,
            decisionBy,
            request.Notes,
            branchCode,
            branchName,
            request.DecisionTemplateName);

        await _creditApplicationRepository.UpdateAsync(creditApplication, cancellationToken);

        _auditLogger.Log(
            AuditEventCategories.Credit,
            AuditEventActions.Reject,
            "Credit application rejected.",
            actorUserId: _currentUserService.UserId,
            targetType: nameof(CreditApplication),
            targetId: creditApplication.Id,
            changes: new
            {
                creditApplication.RejectionReason,
                creditApplication.DecisionTemplateName,
                creditApplication.DecisionBranchCode
            });

        return _mapper.Map<RejectedCreditApplicationResponse>(creditApplication);
    }

    private async Task<(string? Email, string? BranchCode, string? BranchName)> ResolveDecisionMakerAsync(
        CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
            return (null, null, null);

        ApplicationUser? user = await _applicationUserRepository.GetAsync(
            u => u.Id == _currentUserService.UserId.Value,
            cancellationToken: cancellationToken);
        if (user is null)
            return (null, null, null);

        string? email = string.IsNullOrWhiteSpace(user.Email) ? null : user.Email;
        if (!user.BranchId.HasValue)
            return (email, null, null);

        Branch? branch = await _branchRepository.GetAsync(
            b => b.Id == user.BranchId.Value,
            cancellationToken: cancellationToken);

        return (email, branch?.Code, branch?.Name);
    }
}
