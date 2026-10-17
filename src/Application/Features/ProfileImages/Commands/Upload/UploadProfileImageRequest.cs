using Core.Application.Pipelines.Authorization;
using Core.Security.Constants;
using MediatR;

namespace Application.Features.ProfileImages.Commands.Upload;

public class UploadProfileImageRequest : IRequest<UploadProfileImageResponse>, ISecuredRequest
{
    public string[] Roles =>
    [
        GeneralOperationClaims.Customer,
        GeneralOperationClaims.BankStaff,
        GeneralOperationClaims.BankManager
    ];

    public Stream Content { get; set; } = Stream.Null;
    public string ContentType { get; set; } = default!;
    public long Length { get; set; }
}
