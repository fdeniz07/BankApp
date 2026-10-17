namespace Application.Services.ProfileImages;

public interface IProfileImageStorage
{
    Task<string> SaveAsync(Guid userId, Stream content, string contentType, CancellationToken cancellationToken = default);
    Task DeleteIfExistsAsync(string? relativePath, CancellationToken cancellationToken = default);
}
