namespace Application.Services.Auth;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    Guid? CustomerId { get; }
    bool IsAuthenticated { get; }
    bool IsBankStaff { get; }
    bool IsCustomer { get; }
    void EnsureCanAccessCustomer(Guid customerId);
    void EnsureBankStaff();
    Guid GetRequiredCustomerId();
}
