using Application.Features.CreditApplications.Rules;
using Application.Services.Auth;
using Application.Services.Logging;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using MediatR;

namespace Application.Features.CreditApplications.Commands.Cancel;

public class CancelCreditApplicationRequestHandler
    : IRequestHandler<CancelCreditApplicationRequest, CancelledCreditApplicationResponse>
{
    private readonly ICreditApplicationRepository _creditApplicationRepository;
    private readonly IMapper _mapper;
    private readonly CreditApplicationBusinessRules _businessRules;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditLogger _auditLogger;

    public CancelCreditApplicationRequestHandler(
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

    public async Task<CancelledCreditApplicationResponse> Handle(
        CancelCreditApplicationRequest request,
        CancellationToken cancellationToken)
    {
        CreditApplication? creditApplication = await _creditApplicationRepository.GetAsync(
            predicate: c => c.Id == request.Id,
            cancellationToken: cancellationToken);

        await _businessRules.CreditApplicationShouldExistWhenSelected(creditApplication);
        _currentUserService.EnsureCanAccessCustomer(creditApplication!.CustomerId);
        await _businessRules.ApplicationShouldBePendingForCancel(creditApplication);

        creditApplication.Status = CreditApplicationStatus.Cancelled;
        creditApplication.DecisionDate = DateTimeOffset.UtcNow;
        creditApplication.Notes = request.Notes ?? creditApplication.Notes;
        creditApplication.ApprovedAmount = null;
        creditApplication.ApprovedTermMonths = null;
        creditApplication.ApprovedInterestRate = null;
        creditApplication.RejectionReason = null;

        await _creditApplicationRepository.UpdateAsync(creditApplication, cancellationToken);

        _auditLogger.Log(
            AuditEventCategories.Credit,
            AuditEventActions.Cancel,
            "Credit application cancelled.",
            actorUserId: _currentUserService.UserId,
            targetType: nameof(CreditApplication),
            targetId: creditApplication.Id);

        return _mapper.Map<CancelledCreditApplicationResponse>(creditApplication);
    }
}
