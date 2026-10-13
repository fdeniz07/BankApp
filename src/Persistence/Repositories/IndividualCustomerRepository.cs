using Application.Services.Repositories;
using Core.Repositories;
using Domain.Entities;
using Persistence.Contexts;

namespace Persistence.Repositories;

public class IndividualCustomerRepository
    : EfRepositoryBase<IndividualCustomer, BankingDbContext>, IIndividualCustomerRepository
{
    public IndividualCustomerRepository(BankingDbContext context) : base(context)
    {
    }
}
