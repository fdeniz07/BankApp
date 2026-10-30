using Application.Services.Logging;
using Application.Services.Repositories;
using Core.CrossCuttingConcerns.Exceptions.Types;
using Domain.Entities;

namespace Application.Services.Auth;

public class BranchAuthorizationService : IBranchAuthorizationService
{
    private readonly ICurrentUserService _currentUser;
    private readonly ICustomerRepository _customerRepository;
    private readonly IAuditLogger _auditLogger;

    public BranchAuthorizationService(
        ICurrentUserService currentUser,
        ICustomerRepository customerRepository,
        IAuditLogger auditLogger)
    {
        _currentUser = currentUser;
        _customerRepository = customerRepository;
        _auditLogger = auditLogger;
    }

    public bool MustSeeNoBranchData =>
        _currentUser.IsBankStaff
        && !_currentUser.IsBankManager
        && !_currentUser.IsSuperAdmin
        && !_currentUser.BranchId.HasValue;

    public Guid? ResolveListBranchFilter(Guid? requestedBranchId)
    {
        if (_currentUser.IsCustomer && !_currentUser.IsBankStaff)
            return null;

        if (_currentUser.IsSuperAdmin)
            return requestedBranchId;

        if (_currentUser.IsBankManager)
            return requestedBranchId ?? _currentUser.BranchId;

        // Plain staff: locked to own branch
        return _currentUser.BranchId;
    }

    public async Task EnsureCanViewCustomerAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        if (_currentUser.IsCustomer && !_currentUser.IsBankStaff)
        {
            _currentUser.EnsureCanAccessCustomer(customerId);
            return;
        }

        if (_currentUser.IsSuperAdmin || _currentUser.IsBankManager)
            return;

        if (!_currentUser.IsBankStaff)
            throw Deny("You are not allowed to view this customer.", "Customer", customerId);

        Customer? customer = await _customerRepository.GetAsync(c => c.Id == customerId, cancellationToken: cancellationToken);
        if (customer is null)
            return;

        if (!_currentUser.BranchId.HasValue || customer.BranchId != _currentUser.BranchId)
            throw Deny("You can only view customers of your own branch.", "Customer", customerId, customer.BranchId);
    }

    public async Task EnsureCanModifyCustomerAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        if (_currentUser.IsCustomer && !_currentUser.IsBankStaff)
        {
            _currentUser.EnsureCanAccessCustomer(customerId);
            return;
        }

        if (_currentUser.IsSuperAdmin)
        {
            LogSuperAdminWrite("Customer", customerId);
            return;
        }

        if (!_currentUser.IsBankStaff)
            throw Deny("You are not allowed to modify this customer.", "Customer", customerId);

        Customer? customer = await _customerRepository.GetAsync(c => c.Id == customerId, cancellationToken: cancellationToken);
        if (customer is null)
            return;

        if (!_currentUser.BranchId.HasValue || customer.BranchId != _currentUser.BranchId)
            throw Deny("You can only modify customers of your own branch.", "Customer", customerId, customer.BranchId);
    }

    public Task EnsureCanViewApplicationAsync(CreditApplication application, CancellationToken cancellationToken = default)
    {
        if (_currentUser.IsCustomer && !_currentUser.IsBankStaff)
        {
            _currentUser.EnsureCanAccessCustomer(application.CustomerId);
            return Task.CompletedTask;
        }

        if (_currentUser.IsSuperAdmin || _currentUser.IsBankManager)
            return Task.CompletedTask;

        if (!_currentUser.IsBankStaff)
            throw Deny("You are not allowed to view this application.", "CreditApplication", application.Id);

        if (!_currentUser.BranchId.HasValue || application.BranchId != _currentUser.BranchId)
            throw Deny(
                "You can only view applications of your own branch.",
                "CreditApplication",
                application.Id,
                application.BranchId);

        return Task.CompletedTask;
    }

    public Task EnsureCanModifyApplicationAsync(CreditApplication application, CancellationToken cancellationToken = default)
    {
        if (_currentUser.IsCustomer && !_currentUser.IsBankStaff)
        {
            _currentUser.EnsureCanAccessCustomer(application.CustomerId);
            return Task.CompletedTask;
        }

        if (_currentUser.IsSuperAdmin)
        {
            LogSuperAdminWrite("CreditApplication", application.Id);
            return Task.CompletedTask;
        }

        if (!_currentUser.IsBankStaff)
            throw Deny("You are not allowed to modify this application.", "CreditApplication", application.Id);

        // Manager + staff: own branch only for writes
        if (!_currentUser.BranchId.HasValue || application.BranchId != _currentUser.BranchId)
            throw Deny(
                "You can only modify applications of your own branch.",
                "CreditApplication",
                application.Id,
                application.BranchId);

        return Task.CompletedTask;
    }

    public Task EnsureCanManageStaffUserAsync(ApplicationUser targetStaff, CancellationToken cancellationToken = default)
    {
        if (_currentUser.IsSuperAdmin)
        {
            LogSuperAdminWrite("ApplicationUser", targetStaff.Id);
            return Task.CompletedTask;
        }

        if (!_currentUser.IsBankManager)
            throw Deny("Only managers or SuperAdmin can manage staff.", "ApplicationUser", targetStaff.Id);

        if (!_currentUser.BranchId.HasValue)
            throw Deny("Your account has no branch assignment.", "ApplicationUser", targetStaff.Id);

        if (targetStaff.BranchId != _currentUser.BranchId)
            throw Deny(
                "You can only manage staff of your own branch.",
                "ApplicationUser",
                targetStaff.Id,
                targetStaff.BranchId);

        return Task.CompletedTask;
    }

    public void EnsureCanManageBranches()
    {
        _currentUser.EnsureSuperAdmin();
    }

    private AuthorizationException Deny(
        string message,
        string targetType,
        Guid? targetId,
        Guid? resourceBranchId = null)
    {
        _auditLogger.Log(
            AuditEventCategories.Auth,
            AuditEventActions.AccessDenied,
            message,
            actorUserId: _currentUser.UserId,
            targetType: targetType,
            targetId: targetId,
            changes: new
            {
                actorBranchId = _currentUser.BranchId,
                resourceBranchId,
                isSuperAdmin = _currentUser.IsSuperAdmin,
                isManager = _currentUser.IsBankManager
            });

        return new AuthorizationException(message);
    }

    private void LogSuperAdminWrite(string targetType, Guid targetId)
    {
        _auditLogger.Log(
            AuditEventCategories.Auth,
            AuditEventActions.Update,
            "SuperAdmin write across branch scope.",
            actorUserId: _currentUser.UserId,
            targetType: targetType,
            targetId: targetId,
            changes: new { _currentUser.BranchId });
    }
}
