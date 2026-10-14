using Application.Services.Repositories;
using Core.Repositories;
using Core.Security.Entities;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Contexts;

namespace Persistence.Repositories;

public class ApplicationUserRepository : EfRepositoryBase<ApplicationUser, BankingDbContext>, IApplicationUserRepository
{
    public ApplicationUserRepository(BankingDbContext context) : base(context)
    {
    }

    public async Task<List<OperationClaim>> GetClaimsAsync(
        ApplicationUser user,
        CancellationToken cancellationToken = default)
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
