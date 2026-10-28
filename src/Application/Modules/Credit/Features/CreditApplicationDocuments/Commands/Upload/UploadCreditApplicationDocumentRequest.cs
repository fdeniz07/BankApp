using Core.Application.Pipelines.Authorization;
using Core.Security.Constants;
using MediatR;

namespace Application.Modules.Credit.Features.CreditApplicationDocuments.Commands.Upload;

public class UploadCreditApplicationDocumentRequest
    : IRequest<UploadCreditApplicationDocumentResponse>, ISecuredRequest
{
    public string[] Roles =>
    [
        GeneralOperationClaims.Customer,
        GeneralOperationClaims.BankStaff,
        GeneralOperationClaims.BankManager
    ];

    public Guid CreditApplicationId { get; set; }
    public string DocumentType { get; set; } = default!;
    public string OriginalFileName { get; set; } = default!;
    public Stream Content { get; set; } = Stream.Null;
    public string ContentType { get; set; } = default!;
    public long Length { get; set; }
}
