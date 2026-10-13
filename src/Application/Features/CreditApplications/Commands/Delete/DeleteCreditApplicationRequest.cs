using MediatR;

namespace Application.Features.CreditApplications.Commands.Delete;

public class DeleteCreditApplicationRequest : IRequest<DeletedCreditApplicationResponse>
{
    public Guid Id { get; set; }
}
