using Core.Application.Pipelines.Authorization;
using Core.Security.Constants;
using MediatR;

namespace Application.Features.CreditTypes.Queries.GetById;

public class GetByIdCreditTypeRequest : IRequest<GetByIdCreditTypeResponse>, ISecuredRequest
{
    public string[] Roles => [GeneralOperationClaims.Customer, GeneralOperationClaims.BankStaff];
    public Guid Id { get; set; }
}
