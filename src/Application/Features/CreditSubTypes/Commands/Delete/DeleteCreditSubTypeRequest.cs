using MediatR;

namespace Application.Features.CreditSubTypes.Commands.Delete;

public class DeleteCreditSubTypeRequest : IRequest<DeletedCreditSubTypeResponse>
{
    public Guid Id { get; set; }
}
