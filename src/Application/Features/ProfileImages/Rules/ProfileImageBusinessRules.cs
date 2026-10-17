using Application.Features.ProfileImages.Constants;
using Core.Application.Pipelines;
using Core.CrossCuttingConcerns.Exceptions.Types;

namespace Application.Features.ProfileImages.Rules;

public class ProfileImageBusinessRules : BaseBusinessRules
{
    public const long MaxBytes = 2 * 1024 * 1024;

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/webp"
    };

    public Task FileShouldBePresent(Stream? stream, long length)
    {
        if (stream is null || length <= 0)
            throw new BusinessException(ProfileImageMessages.EmptyFile);

        return Task.CompletedTask;
    }

    public Task FileShouldNotExceedMaxSize(long length)
    {
        if (length > MaxBytes)
            throw new BusinessException(ProfileImageMessages.TooLarge);

        return Task.CompletedTask;
    }

    public Task ContentTypeShouldBeAllowed(string? contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType) || !AllowedContentTypes.Contains(contentType.Trim()))
            throw new BusinessException(ProfileImageMessages.InvalidContentType);

        return Task.CompletedTask;
    }

    public async Task ContentShouldMatchDeclaredType(Stream stream, string contentType, CancellationToken cancellationToken)
    {
        byte[] header = new byte[12];
        int read = await stream.ReadAsync(header.AsMemory(0, header.Length), cancellationToken);
        if (stream.CanSeek)
            stream.Position = 0;

        if (read < 3 || !MatchesMagicBytes(header.AsSpan(0, read), contentType))
            throw new BusinessException(ProfileImageMessages.InvalidContent);
    }

    private static bool MatchesMagicBytes(ReadOnlySpan<byte> header, string contentType)
    {
        string type = contentType.Trim().ToLowerInvariant();
        return type switch
        {
            "image/jpeg" => header.Length >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF,
            "image/png" => header.Length >= 8
                && header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47
                && header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A,
            "image/webp" => header.Length >= 12
                && header[0] == (byte)'R' && header[1] == (byte)'I' && header[2] == (byte)'F' && header[3] == (byte)'F'
                && header[8] == (byte)'W' && header[9] == (byte)'E' && header[10] == (byte)'B' && header[11] == (byte)'P',
            _ => false
        };
    }
}
