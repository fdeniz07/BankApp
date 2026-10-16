using Core.Application.Pipelines.Authorization;
using Core.Security.Constants;
using MediatR;

namespace Application.Features.CreditApplications.Commands.Cancel;

public class CancelCreditApplicationRequest : IRequest<CancelledCreditApplicationResponse>, ISecuredRequest
{
    public string[] Roles => [GeneralOperationClaims.Customer, GeneralOperationClaims.BankStaff];
    public Guid Id { get; set; }
    public string? Notes { get; set; }
}
