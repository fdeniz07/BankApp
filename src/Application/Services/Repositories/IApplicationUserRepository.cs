using Core.Repositories;
using Core.Security.Entities;
using Domain.Entities;

namespace Application.Services.Repositories;

public interface IApplicationUserRepository : IAsyncRepository<ApplicationUser>
{
    Task<List<OperationClaim>> GetClaimsAsync(ApplicationUser user, CancellationToken cancellationToken = default);
}
