using Application.Modules.Credit.Features.CreditApplicationDocuments.Constants;
using Application.Services.Repositories;
using Core.Application.Pipelines;
using Core.CrossCuttingConcerns.Exceptions.Types;
using Domain.Entities;
using Domain.Enums;

namespace Application.Modules.Credit.Features.CreditApplicationDocuments.Rules;

public class CreditApplicationDocumentBusinessRules : BaseBusinessRules
{
    public const long MaxBytes = 5 * 1024 * 1024;
    public const int MaxDocumentsPerApplication = 10;

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
        "image/jpeg",
        "image/png"
    };

    private readonly ICreditApplicationDocumentRepository _documentRepository;

    public CreditApplicationDocumentBusinessRules(ICreditApplicationDocumentRepository documentRepository)
    {
        _documentRepository = documentRepository;
    }

    public Task DocumentShouldExist(CreditApplicationDocument? document)
    {
        if (document is null)
            throw new BusinessException(CreditApplicationDocumentMessages.NotExists);

        return Task.CompletedTask;
    }

    public Task FileShouldBePresent(Stream? stream, long length)
    {
        if (stream is null || length <= 0)
            throw new BusinessException(CreditApplicationDocumentMessages.EmptyFile);

        return Task.CompletedTask;
    }

    public Task FileShouldNotExceedMaxSize(long length)
    {
        if (length > MaxBytes)
            throw new BusinessException(CreditApplicationDocumentMessages.TooLarge);

        return Task.CompletedTask;
    }

    public Task ContentTypeShouldBeAllowed(string? contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType) || !AllowedContentTypes.Contains(contentType.Trim()))
            throw new BusinessException(CreditApplicationDocumentMessages.InvalidContentType);

        return Task.CompletedTask;
    }

    public Task DocumentTypeShouldBeValid(string? documentTypeName)
    {
        if (string.IsNullOrWhiteSpace(documentTypeName)
            || !CreditDocumentType.TryFromName(documentTypeName.Trim(), ignoreCase: true, out _))
            throw new BusinessException(CreditApplicationDocumentMessages.InvalidDocumentType);

        return Task.CompletedTask;
    }

    public Task ApplicationShouldBePendingForUpload(CreditApplication application)
    {
        if (application.Status != CreditApplicationStatus.Pending)
            throw new BusinessException(CreditApplicationDocumentMessages.OnlyPendingCanUpload);

        return Task.CompletedTask;
    }

    public async Task ApplicationShouldNotExceedDocumentLimit(Guid applicationId)
    {
        int count = await _documentRepository.CountAsync(d => d.CreditApplicationId == applicationId && !d.IsDeleted);
        if (count >= MaxDocumentsPerApplication)
            throw new BusinessException(CreditApplicationDocumentMessages.MaxDocumentsExceeded);
    }

    public async Task ContentShouldMatchDeclaredType(Stream stream, string contentType, CancellationToken cancellationToken)
    {
        byte[] header = new byte[8];
        int read = await stream.ReadAsync(header.AsMemory(0, header.Length), cancellationToken);
        if (stream.CanSeek)
            stream.Position = 0;

        if (read < 4 || !MatchesMagicBytes(header.AsSpan(0, read), contentType))
            throw new BusinessException(CreditApplicationDocumentMessages.InvalidContent);
    }

    private static bool MatchesMagicBytes(ReadOnlySpan<byte> header, string contentType)
    {
        string type = contentType.Trim().ToLowerInvariant();
        return type switch
        {
            "application/pdf" => header.Length >= 4
                && header[0] == (byte)'%' && header[1] == (byte)'P' && header[2] == (byte)'D' && header[3] == (byte)'F',
            "image/jpeg" => header.Length >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF,
            "image/png" => header.Length >= 8
                && header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47
                && header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A,
            _ => false
        };
    }
}
