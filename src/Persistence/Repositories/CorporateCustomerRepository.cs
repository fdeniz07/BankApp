using Application.Services.Repositories;
using Core.Repositories;
using Domain.Entities;
using Persistence.Contexts;

namespace Persistence.Repositories;

public class CorporateCustomerRepository
    : EfRepositoryBase<CorporateCustomer, BankingDbContext>, ICorporateCustomerRepository
{
    public CorporateCustomerRepository(BankingDbContext context) : base(context)
    {
    }
}
