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
    private readonly IMapper _mapper;
    private readonly CreditApplicationBusinessRules _businessRules;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditLogger _auditLogger;

    public RejectCreditApplicationRequestHandler(
        ICreditApplicationRepository creditApplicationRepository,
        IApplicationUserRepository applicationUserRepository,
        IMapper mapper,
        CreditApplicationBusinessRules businessRules,
        ICurrentUserService currentUserService,
        IAuditLogger auditLogger)
    {
        _creditApplicationRepository = creditApplicationRepository;
        _applicationUserRepository = applicationUserRepository;
        _mapper = mapper;
        _businessRules = businessRules;
        _currentUserService = currentUserService;
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
        await _businessRules.ApplicationShouldBePending(creditApplication!);

        string? decisionBy = await ResolveDecisionByEmailAsync(cancellationToken);
        creditApplication!.Reject(request.RejectionReason, decisionBy, request.Notes);

        await _creditApplicationRepository.UpdateAsync(creditApplication, cancellationToken);

        _auditLogger.Log(
            AuditEventCategories.Credit,
            AuditEventActions.Reject,
            "Credit application rejected.",
            actorUserId: _currentUserService.UserId,
            targetType: nameof(CreditApplication),
            targetId: creditApplication.Id,
            changes: new { creditApplication.RejectionReason });

        return _mapper.Map<RejectedCreditApplicationResponse>(creditApplication);
    }

    private async Task<string?> ResolveDecisionByEmailAsync(CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
            return null;

        ApplicationUser? user = await _applicationUserRepository.GetAsync(
            u => u.Id == _currentUserService.UserId.Value,
            cancellationToken: cancellationToken);

        return string.IsNullOrWhiteSpace(user?.Email) ? null : user.Email;
    }
}
