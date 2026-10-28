using Application.Services.CreditDocuments;

namespace WebApi.Services;

/// <summary>
/// Stores credit application files under App_Data (not wwwroot) so StaticFiles cannot serve them.
/// </summary>
public class LocalCreditDocumentStorage : ICreditDocumentStorage
{
    private const string RelativeRoot = "App_Data/credit-documents";
    private readonly IWebHostEnvironment _environment;

    public LocalCreditDocumentStorage(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    private string RootDirectory =>
        Path.GetFullPath(Path.Combine(_environment.ContentRootPath, RelativeRoot));

    public async Task<string> SaveAsync(
        Guid applicationId,
        Guid documentId,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        string extension = contentType.Trim().ToLowerInvariant() switch
        {
            "application/pdf" => ".pdf",
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            _ => ".bin"
        };

        string relativeKey = $"{applicationId:N}/{documentId:N}{extension}";
        string absolutePath = ResolveAbsolutePath(relativeKey);
        Directory.CreateDirectory(Path.GetDirectoryName(absolutePath)!);

        await using FileStream fileStream = new(absolutePath, FileMode.Create, FileAccess.Write, FileShare.None);
        if (content.CanSeek)
            content.Position = 0;
        await content.CopyToAsync(fileStream, cancellationToken);

        return relativeKey.Replace('\\', '/');
    }

    public Task<Stream> OpenReadAsync(string storagePath, CancellationToken cancellationToken = default)
    {
        string absolutePath = ResolveAbsolutePath(storagePath);
        if (!File.Exists(absolutePath))
            throw new FileNotFoundException("Credit document file missing on disk.", absolutePath);

        Stream stream = new FileStream(absolutePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Task.FromResult(stream);
    }

    public Task DeleteIfExistsAsync(string? storagePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(storagePath))
            return Task.CompletedTask;

        string absolutePath = ResolveAbsolutePath(storagePath);
        if (File.Exists(absolutePath))
            File.Delete(absolutePath);

        return Task.CompletedTask;
    }

    private string ResolveAbsolutePath(string storagePath)
    {
        string trimmed = storagePath.Trim().TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        string absolutePath = Path.GetFullPath(Path.Combine(RootDirectory, trimmed));
        string root = RootDirectory.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if (!absolutePath.StartsWith(root, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(absolutePath, RootDirectory.TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Invalid credit document storage path.");

        return absolutePath;
    }
}
