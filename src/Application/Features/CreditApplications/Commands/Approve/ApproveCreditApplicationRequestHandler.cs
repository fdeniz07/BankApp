using Application.Features.CreditApplications.Rules;
using Application.Services.Auth;
using Application.Services.Logging;
using Application.Services.Repositories;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using MediatR;

namespace Application.Features.CreditApplications.Commands.Approve;

public class ApproveCreditApplicationRequestHandler
    : IRequestHandler<ApproveCreditApplicationRequest, ApprovedCreditApplicationResponse>
{
    private readonly ICreditApplicationRepository _creditApplicationRepository;
    private readonly IMapper _mapper;
    private readonly CreditApplicationBusinessRules _businessRules;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditLogger _auditLogger;

    public ApproveCreditApplicationRequestHandler(
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

    public async Task<ApprovedCreditApplicationResponse> Handle(
        ApproveCreditApplicationRequest request,
        CancellationToken cancellationToken)
    {
        CreditApplication? creditApplication = await _creditApplicationRepository.GetAsync(
            predicate: c => c.Id == request.Id,
            cancellationToken: cancellationToken);

        await _businessRules.CreditApplicationShouldExistWhenSelected(creditApplication);
        await _businessRules.ApplicationShouldBePending(creditApplication!);
        await _businessRules.RequestShouldBeWithinCreditSubTypeLimits(
            creditApplication!.CreditSubTypeId,
            request.ApprovedAmount,
            request.ApprovedTermMonths);

        creditApplication.Status = CreditApplicationStatus.Approved;
        creditApplication.ApprovedAmount = request.ApprovedAmount;
        creditApplication.ApprovedTermMonths = request.ApprovedTermMonths;
        creditApplication.ApprovedInterestRate = request.ApprovedInterestRate;
        creditApplication.DecisionDate = DateTimeOffset.UtcNow;
        creditApplication.DecisionBy = request.DecisionBy;
        creditApplication.Notes = request.Notes ?? creditApplication.Notes;
        creditApplication.RejectionReason = null;

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
                creditApplication.ApprovedInterestRate
            });

        return _mapper.Map<ApprovedCreditApplicationResponse>(creditApplication);
    }
}
