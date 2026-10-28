using Core.Application.Pipelines.Authorization;
using Core.Security.Constants;
using MediatR;

namespace Application.Modules.Credit.Features.CreditApplicationDocuments.Queries.Download;

public class DownloadCreditApplicationDocumentRequest
    : IRequest<DownloadCreditApplicationDocumentResponse>, ISecuredRequest
{
    public string[] Roles =>
    [
        GeneralOperationClaims.Customer,
        GeneralOperationClaims.BankStaff,
        GeneralOperationClaims.BankManager
    ];

    public Guid CreditApplicationId { get; set; }
    public Guid DocumentId { get; set; }
}
