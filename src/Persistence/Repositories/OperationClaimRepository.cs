using Application.Services.Repositories;
using Core.Repositories;
using Core.Security.Entities;
using Persistence.Contexts;

namespace Persistence.Repositories;

public class OperationClaimRepository : EfRepositoryBase<OperationClaim, BankingDbContext>, IOperationClaimRepository
{
    public OperationClaimRepository(BankingDbContext context) : base(context)
    {
    }
}
