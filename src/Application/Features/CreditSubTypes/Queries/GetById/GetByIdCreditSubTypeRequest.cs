using MediatR;

namespace Application.Features.CreditSubTypes.Queries.GetById;

public class GetByIdCreditSubTypeRequest : IRequest<GetByIdCreditSubTypeResponse>
{
    public Guid Id { get; set; }
}
