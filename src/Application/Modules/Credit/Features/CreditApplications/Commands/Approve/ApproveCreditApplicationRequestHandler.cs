using Application.Modules.Credit.Features.CreditApplicationDocuments;
using Application.Modules.Credit.Features.CreditApplications.ApprovalAuthority;
using Application.Modules.Credit.Features.CreditApplications.Rules;
using Application.Services.Auth;
using Application.Services.Logging;
using Application.Services.Repositories;
using Application.Services.Search;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.Extensions.Options;

namespace Application.Modules.Credit.Features.CreditApplications.Commands.Approve;

public class ApproveCreditApplicationRequestHandler
    : IRequestHandler<ApproveCreditApplicationRequest, ApprovedCreditApplicationResponse>
{
    private readonly ICreditApplicationRepository _creditApplicationRepository;
    private readonly ICreditApplicationDocumentRepository _documentRepository;
    private readonly IApplicationUserRepository _applicationUserRepository;
    private readonly IBranchRepository _branchRepository;
    private readonly IMapper _mapper;
    private readonly CreditApplicationBusinessRules _businessRules;
    private readonly ICurrentUserService _currentUserService;
    private readonly IBranchAuthorizationService _branchAuthorization;
    private readonly IAuditLogger _auditLogger;
    private readonly CreditDocumentsOptions _documentsOptions;
    private readonly ApprovalAuthorityOptions _authorityOptions;
    private readonly ICreditApplicationSearchService _search;

    public ApproveCreditApplicationRequestHandler(
        ICreditApplicationRepository creditApplicationRepository,
        ICreditApplicationDocumentRepository documentRepository,
        IApplicationUserRepository applicationUserRepository,
        IBranchRepository branchRepository,
        IMapper mapper,
        CreditApplicationBusinessRules businessRules,
        ICurrentUserService currentUserService,
        IBranchAuthorizationService branchAuthorization,
        IAuditLogger auditLogger,
        IOptions<CreditDocumentsOptions> documentsOptions,
        IOptions<ApprovalAuthorityOptions> authorityOptions,
        ICreditApplicationSearchService search)
    {
        _creditApplicationRepository = creditApplicationRepository;
        _documentRepository = documentRepository;
        _applicationUserRepository = applicationUserRepository;
        _branchRepository = branchRepository;
        _mapper = mapper;
        _businessRules = businessRules;
        _currentUserService = currentUserService;
        _branchAuthorization = branchAuthorization;
        _auditLogger = auditLogger;
        _documentsOptions = documentsOptions.Value;
        _authorityOptions = authorityOptions.Value;
        _search = search;
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

        if (_documentsOptions.ApproveGateMode == ApproveDocumentGateMode.Hard)
        {
            var documents = await _documentRepository.GetListAsync(
                predicate: d => d.CreditApplicationId == creditApplication.Id && !d.IsDeleted,
                size: 50,
                cancellationToken: cancellationToken);
            IEnumerable<string> types = documents.Items.Select(d => d.DocumentType.Name);
            await _businessRules.RequiredDocumentsShouldExistWhenHardGate(true, types);
        }

        ApplicationUser? actor = await LoadActorAsync(cancellationToken);
        decimal? branchCap = null;
        string? branchCapCurrency = null;
        if (actor?.BranchId is Guid actorBranchId)
        {
            Branch? actorBranch = await _branchRepository.GetAsync(
                b => b.Id == actorBranchId,
                cancellationToken: cancellationToken);
            branchCap = actorBranch?.MaxApprovalAmount;
            branchCapCurrency = actorBranch?.MaxApprovalCurrencyCode;
        }

        string applicationCurrency = await _businessRules.ResolvePackageCurrencyCodeAsync(
            creditApplication!.CreditSubTypeId,
            cancellationToken);

        decimal? maxAmount = ApprovalAuthorityEvaluator.ResolveMaxAmount(
            _authorityOptions,
            _currentUserService.IsBankManager,
            _currentUserService.IsSuperAdmin,
            actor?.Position,
            branchCap,
            branchCapCurrency,
            applicationCurrency);
        await _businessRules.ApprovedAmountShouldBeWithinApprovalAuthority(
            request.ApprovedAmount,
            maxAmount);

        (string? decisionBy, string? branchCode, string? branchName) =
            await ResolveDecisionMakerAsync(actor, cancellationToken);

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
        await _search.UpsertFromApplicationAsync(creditApplication, cancellationToken);

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

    private async Task<ApplicationUser?> LoadActorAsync(CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue)
            return null;

        return await _applicationUserRepository.GetAsync(
            u => u.Id == _currentUserService.UserId.Value,
            cancellationToken: cancellationToken);
    }

    private async Task<(string? Email, string? BranchCode, string? BranchName)> ResolveDecisionMakerAsync(
        ApplicationUser? user,
        CancellationToken cancellationToken)
    {
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
