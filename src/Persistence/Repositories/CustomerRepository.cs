using Application.Services.Repositories;
using Core.Repositories;
using Domain.Entities;
using Persistence.Contexts;

namespace Persistence.Repositories;

public class CustomerRepository : EfRepositoryBase<Customer, BankingDbContext>, ICustomerRepository
{
    public CustomerRepository(BankingDbContext context) : base(context)
    {
    }
}
