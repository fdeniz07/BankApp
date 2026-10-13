using MediatR;

namespace Application.Features.CorporateCustomers.Commands.Delete;

public class DeleteCorporateCustomerRequest : IRequest<DeletedCorporateCustomerResponse>
{
    public Guid Id { get; set; }
}
