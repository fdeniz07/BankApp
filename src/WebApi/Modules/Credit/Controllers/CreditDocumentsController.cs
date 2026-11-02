using Application.Modules.Credit.Features.CreditApplicationDocuments;
using Application.Modules.Credit.Features.CreditApplications.ApprovalAuthority;
using Application.Services.Auth;
using Application.Services.Repositories;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using WebApi.Constants;
using WebApi.Controllers;

namespace WebApi.Modules.Credit.Controllers;

[Route("api/[controller]")]
[ApiController]
[EnableRateLimiting(RateLimitPolicies.Fixed)]
public class CreditDocumentsController : BaseController
{
    private readonly CreditDocumentsOptions _documentsOptions;
    private readonly ApprovalAuthorityOptions _authorityOptions;
    private readonly ICurrentUserService _currentUser;
    private readonly IApplicationUserRepository _users;
    private readonly IBranchRepository _branches;

    public CreditDocumentsController(
        IOptions<CreditDocumentsOptions> documentsOptions,
        IOptions<ApprovalAuthorityOptions> authorityOptions,
        ICurrentUserService currentUser,
        IApplicationUserRepository users,
        IBranchRepository branches)
    {
        _documentsOptions = documentsOptions.Value;
        _authorityOptions = authorityOptions.Value;
        _currentUser = currentUser;
        _users = users;
        _branches = branches;
    }

    /// <summary>Staff decision UX: document gate mode + approval authority for the current user.</summary>
    [HttpGet("config")]
    public async Task<IActionResult> GetConfig(CancellationToken cancellationToken)
    {
        ApprovalAuthorityLimit limit = new();
        if (_authorityOptions.Enabled && _currentUser.UserId.HasValue)
        {
            ApplicationUser? user = await _users.GetAsync(
                u => u.Id == _currentUser.UserId.Value,
                cancellationToken: cancellationToken);
            decimal? branchCap = null;
            if (user?.BranchId is Guid branchId)
            {
                Branch? branch = await _branches.GetAsync(
                    b => b.Id == branchId,
                    cancellationToken: cancellationToken);
                branchCap = branch?.MaxApprovalAmount;
            }

            limit = ApprovalAuthorityEvaluator.Resolve(
                _authorityOptions,
                _currentUser.IsBankManager,
                _currentUser.IsSuperAdmin,
                user?.Position,
                branchCap);
        }

        return Ok(new CreditDecisionConfigResponse(
            ApproveGateMode: _documentsOptions.ApproveGateMode.ToString(),
            SoftApproveGateEnabled: _documentsOptions.ApproveGateMode == ApproveDocumentGateMode.Soft,
            ApprovalAuthorityEnabled: _authorityOptions.Enabled,
            MaxApprovalAmount: limit.AppliedMax,
            PositionMaxApprovalAmount: limit.PositionMax,
            BranchMaxApprovalAmount: limit.BranchMax));
    }

    public sealed record CreditDecisionConfigResponse(
        string ApproveGateMode,
        bool SoftApproveGateEnabled,
        bool ApprovalAuthorityEnabled,
        decimal? MaxApprovalAmount,
        decimal? PositionMaxApprovalAmount,
        decimal? BranchMaxApprovalAmount);
}
