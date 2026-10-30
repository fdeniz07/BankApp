using Application.Modules.Credit.Features.CreditApplications.Rules;
using Application.Services.Auth;
using Application.Services.Logging;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;

namespace Application.Modules.Credit.Features.CreditApplications.Commands.Approve;

public class ApproveCreditApplicationRequestHandler
    : IRequestHandler<ApproveCreditApplicationRequest, ApprovedCreditApplicationResponse>
{
    private readonly ICreditApplicationRepository _creditApplicationRepository;
    private readonly IApplicationUserRepository _applicationUserRepository;
    private readonly IBranchRepository _branchRepository;
    private readonly IMapper _mapper;
    private readonly CreditApplicationBusinessRules _businessRules;
    private readonly ICurrentUserService _currentUserService;
    private readonly IBranchAuthorizationService _branchAuthorization;
    private readonly IAuditLogger _auditLogger;

    public ApproveCreditApplicationRequestHandler(
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

    public async Task<ApprovedCreditApplicationResponse> Handle(
        ApproveCreditApplicationRequest request,
        CancellationToken cancellationToken)
    {
        CreditApplication? creditApplication = await _creditApplicationRepository.GetAsync(
            predicate: c => c.Id == request.Id,
            cancellationToken: cancellationToken);

        await _businessRules.CreditApplicationShouldExistWhenSelected(creditApplication);
        await _branchAuthorization.EnsureCanModifyApplicationAsync(creditApplication!, cancellationToken);
        await _businessRules.ApplicationShouldBePending(creditApplication!);
        await _businessRules.RequestShouldBeWithinCreditSubTypeLimits(
            creditApplication!.CreditSubTypeId,
            request.ApprovedAmount,
            request.ApprovedTermMonths);

        (string? decisionBy, string? branchCode, string? branchName) =
            await ResolveDecisionMakerAsync(cancellationToken);

        creditApplication.Approve(
            request.ApprovedAmount,
            request.ApprovedTermMonths,
            request.ApprovedInterestRate,
            decisionBy,
            request.Notes,
            branchCode,
            branchName,
            request.DecisionTemplateName);

        await _creditApplicationRepository.UpdateAsync(creditApplication, cancellationToken);

        _auditLogger.Log(
            AuditEventCategories.Credit,
            AuditEventActions.Approve,
            "Credit application approved.",
            actorUserId: _currentUserService.UserId,
            targetType: nameof(CreditApplication),
            targetId: creditApplication.Id,
            changes: new
            {
                creditApplication.ApprovedAmount,
                creditApplication.ApprovedTermMonths,
                creditApplication.ApprovedInterestRate,
                creditApplication.DecisionTemplateName,
                creditApplication.DecisionBranchCode
            });

        return _mapper.Map<ApprovedCreditApplicationResponse>(creditApplication);
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
