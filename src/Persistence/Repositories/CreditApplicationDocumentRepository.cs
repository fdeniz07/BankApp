using Application.Services.Repositories;
using Core.Repositories;
using Domain.Entities;
using Persistence.Contexts;

namespace Persistence.Repositories;

public class CreditApplicationDocumentRepository
    : EfRepositoryBase<CreditApplicationDocument, BankingDbContext>, ICreditApplicationDocumentRepository
{
    public CreditApplicationDocumentRepository(BankingDbContext context) : base(context)
    {
    }
}
