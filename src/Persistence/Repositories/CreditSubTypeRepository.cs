using Application.Services.Repositories;
using Core.Repositories;
using Domain.Entities;
using Persistence.Contexts;

namespace Persistence.Repositories;

public class CreditSubTypeRepository : EfRepositoryBase<CreditSubType, BankingDbContext>, ICreditSubTypeRepository
{
    public CreditSubTypeRepository(BankingDbContext context) : base(context)
    {
    }
}
