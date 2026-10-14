using Core.Repositories;
using Core.Security.Entities;

namespace Application.Services.Repositories;

public interface IUserRepository : IAsyncRepository<User>
{
    Task<List<OperationClaim>> GetClaimsAsync(User user, CancellationToken cancellationToken = default);
}
