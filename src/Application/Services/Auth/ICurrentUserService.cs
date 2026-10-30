namespace Application.Services.Auth;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    Guid? CustomerId { get; }
    Guid? BranchId { get; }
    bool IsAuthenticated { get; }
    bool IsBankStaff { get; }
    bool IsBankManager { get; }
    bool IsSuperAdmin { get; }
    bool IsCustomer { get; }

    /// <summary>
    /// Legacy helper: customers may only access own data; any bank staff may access
    /// (branch-scoped checks use <see cref="IBranchAuthorizationService"/>).
    /// </summary>
    void EnsureCanAccessCustomer(Guid customerId);

    void EnsureBankStaff();
    void EnsureSuperAdmin();
    Guid GetRequiredCustomerId();
}
