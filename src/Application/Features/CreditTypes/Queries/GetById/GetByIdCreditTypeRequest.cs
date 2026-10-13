using MediatR;

namespace Application.Features.CreditTypes.Queries.GetById;

public class GetByIdCreditTypeRequest : IRequest<GetByIdCreditTypeResponse>
{
    public Guid Id { get; set; }
}
