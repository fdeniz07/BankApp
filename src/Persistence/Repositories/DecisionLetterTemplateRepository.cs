using Application.Services.Repositories;
using Core.Repositories;
using Domain.Entities;
using Persistence.Contexts;

namespace Persistence.Repositories;

public class DecisionLetterTemplateRepository
    : EfRepositoryBase<DecisionLetterTemplate, BankingDbContext>, IDecisionLetterTemplateRepository
{
    public DecisionLetterTemplateRepository(BankingDbContext context) : base(context)
    {
    }
}
