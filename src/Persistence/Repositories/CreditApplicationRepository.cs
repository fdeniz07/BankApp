using Application.Services.Repositories;
using Core.Repositories;
using Domain.Entities;
using Persistence.Contexts;

namespace Persistence.Repositories;

public class CreditApplicationRepository
    : EfRepositoryBase<CreditApplication, BankingDbContext>, ICreditApplicationRepository
{
    public CreditApplicationRepository(BankingDbContext context) : base(context)
    {
    }
}
