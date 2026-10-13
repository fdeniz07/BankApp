using Core.Repositories;
using Domain.Entities;

namespace Application.Services.Repositories;

public interface ICreditTypeRepository : IAsyncRepository<CreditType>
{
}
