using Application.Services.Auth;
using Core.CrossCuttingConcerns.Exceptions.Types;
using Core.Security.Constants;
using Core.Security.Extensions;

namespace WebApi.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId => _httpContextAccessor.HttpContext?.User.GetUserId();

    public Guid? CustomerId => _httpContextAccessor.HttpContext?.User.GetCustomerId();

    public Guid? BranchId => _httpContextAccessor.HttpContext?.User.GetBranchId();

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true;

    public bool IsBankStaff =>
        _httpContextAccessor.HttpContext?.User.ClaimRoles()?.Contains(GeneralOperationClaims.BankStaff) == true
        || IsBankManager
        || IsSuperAdmin;

    public bool IsBankManager =>
        _httpContextAccessor.HttpContext?.User.ClaimRoles()?.Contains(GeneralOperationClaims.BankManager) == true
        || IsSuperAdmin;

    public bool IsSuperAdmin =>
        _httpContextAccessor.HttpContext?.User.ClaimRoles()?.Contains(GeneralOperationClaims.SuperAdmin) == true;

    public bool IsCustomer =>
        _httpContextAccessor.HttpContext?.User.ClaimRoles()?.Contains(GeneralOperationClaims.Customer) == true;

    public void EnsureCanAccessCustomer(Guid customerId)
    {
        if (IsBankStaff)
            return;

        if (!IsCustomer || CustomerId != customerId)
            throw new AuthorizationException("You can only access your own data.");
    }

    public void EnsureBankStaff()
    {
        if (!IsBankStaff)
            throw new AuthorizationException("Only bank staff can perform this operation.");
    }

    public void EnsureSuperAdmin()
    {
        if (!IsSuperAdmin)
            throw new AuthorizationException("Only SuperAdmin can perform this operation.");
    }

    public Guid GetRequiredCustomerId()
    {
        if (!CustomerId.HasValue)
            throw new AuthorizationException("Customer account is not linked to a customer profile.");

        return CustomerId.Value;
    }
}
