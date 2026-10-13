using Core.Repositories;
using Domain.Entities;

namespace Application.Services.Repositories;

public interface ICreditApplicationRepository : IAsyncRepository<CreditApplication>
{
}
