using MediatR;

namespace Application.Features.CreditApplications.Queries.GetById;

public class GetByIdCreditApplicationRequest : IRequest<GetByIdCreditApplicationResponse>
{
    public Guid Id { get; set; }
}
