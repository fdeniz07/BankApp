using Core.Security.Entities;

namespace Core.Security.JWT;

public interface ITokenHelper
{
    AccessToken CreateToken(
        User user,
        IList<OperationClaim> operationClaims,
        Guid? customerId = null,
        string? displayName = null,
        string? customerCategory = null,
        int? expirationMinutes = null,
        Guid? branchId = null);
}
