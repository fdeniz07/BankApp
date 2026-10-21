using Application.Features.CreditApplications.Rules;
using Application.Services.Auth;
using Application.Services.Logging;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;

namespace Application.Features.CreditApplications.Commands.Reject;

public class RejectCreditApplicationRequestHandler
    : IRequestHandler<RejectCreditApplicationRequest, RejectedCreditApplicationResponse>
{
    private readonly ICreditApplicationRepository _creditApplicationRepository;
    private readonly IMapper _mapper;
    private readonly CreditApplicationBusinessRules _businessRules;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditLogger _auditLogger;

    public RejectCreditApplicationRequestHandler(
        ICreditApplicationRepository creditApplicationRepository,
        IMapper mapper,
        CreditApplicationBusinessRules businessRules,
        ICurrentUserService currentUserService,
        IAuditLogger auditLogger)
    {
        _creditApplicationRepository = creditApplicationRepository;
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

        creditApplication!.Reject(request.RejectionReason, request.DecisionBy, request.Notes);

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
}
