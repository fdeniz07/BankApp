using Core.Repositories;
using Domain.Entities;

namespace Application.Services.Repositories;

public interface ICreditApplicationDocumentRepository : IAsyncRepository<CreditApplicationDocument>
{
}
