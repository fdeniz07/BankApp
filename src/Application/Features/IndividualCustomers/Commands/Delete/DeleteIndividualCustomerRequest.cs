using MediatR;

namespace Application.Features.IndividualCustomers.Commands.Delete;

public class DeleteIndividualCustomerRequest : IRequest<DeletedIndividualCustomerResponse>
{
    public Guid Id { get; set; }
}
