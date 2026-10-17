using Application.Services.ProfileImages;

namespace WebApi.Services;

public class LocalProfileImageStorage : IProfileImageStorage
{
    private readonly IWebHostEnvironment _environment;
    private const string RelativeRoot = "uploads/profiles";

    public LocalProfileImageStorage(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    private string WebRoot =>
        string.IsNullOrWhiteSpace(_environment.WebRootPath)
            ? Path.Combine(_environment.ContentRootPath, "wwwroot")
            : _environment.WebRootPath;

    public async Task<string> SaveAsync(
        Guid userId,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        string extension = contentType.Trim().ToLowerInvariant() switch
        {
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            "image/webp" => ".webp",
            _ => ".bin"
        };

        string directory = Path.Combine(WebRoot, RelativeRoot);
        Directory.CreateDirectory(directory);

        // Unique name so re-upload does not delete the file just written (same path as previous).
        string fileName = $"{userId:N}_{Guid.NewGuid():N}{extension}";
        string absolutePath = Path.Combine(directory, fileName);

        await using FileStream fileStream = new(absolutePath, FileMode.Create, FileAccess.Write, FileShare.None);
        if (content.CanSeek)
            content.Position = 0;
        await content.CopyToAsync(fileStream, cancellationToken);

        return $"/{RelativeRoot}/{fileName}".Replace('\\', '/');
    }

    public Task DeleteIfExistsAsync(string? relativePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return Task.CompletedTask;

        string trimmed = relativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        // Only allow deletes under uploads/profiles
        if (!trimmed.StartsWith(RelativeRoot.Replace('/', Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase))
            return Task.CompletedTask;

        string absolutePath = Path.Combine(WebRoot, trimmed);
        if (File.Exists(absolutePath))
            File.Delete(absolutePath);

        return Task.CompletedTask;
    }
}
