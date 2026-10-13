using MediatR;

namespace Application.Features.IndividualCustomers.Queries.GetById;

public class GetByIdIndividualCustomerRequest : IRequest<GetByIdIndividualCustomerResponse>
{
    public Guid Id { get; set; }
}
