using Application.Services.Repositories;
using Core.Repositories;
using Domain.Entities;
using Persistence.Contexts;

namespace Persistence.Repositories;

public class CreditTypeRepository : EfRepositoryBase<CreditType, BankingDbContext>, ICreditTypeRepository
{
    public CreditTypeRepository(BankingDbContext context) : base(context)
    {
    }
}
