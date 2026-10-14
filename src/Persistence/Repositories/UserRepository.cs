using Application.Services.Repositories;
using Core.Repositories;
using Core.Security.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Contexts;

namespace Persistence.Repositories;

public class UserRepository : EfRepositoryBase<User, BankingDbContext>, IUserRepository
{
    public UserRepository(BankingDbContext context) : base(context)
    {
    }

    public async Task<List<OperationClaim>> GetClaimsAsync(User user, CancellationToken cancellationToken = default)
    {
        return await Context.UserOperationClaims
            .AsNoTracking()
            .Where(uoc => uoc.UserId == user.Id && !uoc.IsDeleted)
            .Include(uoc => uoc.OperationClaim)
            .Select(uoc => uoc.OperationClaim)
            .Where(oc => !oc.IsDeleted)
            .ToListAsync(cancellationToken);
    }
}
