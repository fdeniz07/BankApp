using Application.Services.Repositories;
using Core.Repositories;
using Domain.Entities;
using Persistence.Contexts;

namespace Persistence.Repositories;

public class BranchRepository
    : EfRepositoryBase<Branch, BankingDbContext>, IBranchRepository
{
    public BranchRepository(BankingDbContext context) : base(context)
    {
    }
}
