using MediatR;

namespace Application.Features.CorporateCustomers.Queries.GetById;

public class GetByIdCorporateCustomerRequest : IRequest<GetByIdCorporateCustomerResponse>
{
    public Guid Id { get; set; }
}
