using Core.Application.Pipelines.Authorization;
using Core.Security.Constants;
using MediatR;

namespace Application.Modules.Credit.Features.CreditApplicationDocuments.Queries.GetList;

public class GetListCreditApplicationDocumentRequest
    : IRequest<IReadOnlyList<GetListCreditApplicationDocumentItemDto>>, ISecuredRequest
{
    public string[] Roles =>
    [
        GeneralOperationClaims.Customer,
        GeneralOperationClaims.BankStaff,
        GeneralOperationClaims.BankManager
    ];

    public Guid CreditApplicationId { get; set; }
}
