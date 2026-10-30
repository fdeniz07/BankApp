using Domain.Entities;

namespace Application.Services.Auth;

public interface IBranchAuthorizationService
{
    /// <summary>
    /// Branch filter for list queries. Null means no branch restriction (manager/superadmin seeing all,
    /// or customer lists which use CustomerId instead).
    /// Staff (non-manager): forced to own branch. Missing BranchId → empty filter sentinel.
    /// </summary>
    Guid? ResolveListBranchFilter(Guid? requestedBranchId);

    /// <summary>True when the caller must not see any branch-scoped rows (staff without BranchId).</summary>
    bool MustSeeNoBranchData { get; }

    Task EnsureCanViewCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task EnsureCanModifyCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);

    Task EnsureCanViewApplicationAsync(CreditApplication application, CancellationToken cancellationToken = default);
    Task EnsureCanModifyApplicationAsync(CreditApplication application, CancellationToken cancellationToken = default);

    Task EnsureCanManageStaffUserAsync(ApplicationUser targetStaff, CancellationToken cancellationToken = default);
    void EnsureCanManageBranches();
}
