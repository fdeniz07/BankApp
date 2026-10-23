using Application.Services.Repositories;
using Core.Repositories;
using Domain.Entities;
using Persistence.Contexts;

namespace Persistence.Repositories;

public class CreditApplicationSagaRepository
    : EfRepositoryBase<CreditApplicationSaga, BankingDbContext>, ICreditApplicationSagaRepository
{
    public CreditApplicationSagaRepository(BankingDbContext context) : base(context)
    {
    }
}
