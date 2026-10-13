using MediatR;

namespace Application.Features.CreditTypes.Commands.Delete;

public class DeleteCreditTypeRequest : IRequest<DeletedCreditTypeResponse>
{
    public Guid Id { get; set; }
}
