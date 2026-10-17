namespace Application.Features.ProfileImages.Commands.Upload;

public class UploadProfileImageResponse
{
    public Guid UserId { get; set; }
    public string ProfileImageUrl { get; set; } = default!;
}
