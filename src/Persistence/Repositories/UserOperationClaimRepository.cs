using Application.Services.Repositories;
using Core.Repositories;
using Core.Security.Entities;
using Persistence.Contexts;

namespace Persistence.Repositories;

public class UserOperationClaimRepository : EfRepositoryBase<UserOperationClaim, BankingDbContext>, IUserOperationClaimRepository
{
    public UserOperationClaimRepository(BankingDbContext context) : base(context)
    {
    }
}
