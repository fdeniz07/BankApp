namespace Application.Features.ProfileImages.Constants;

public static class ProfileImageMessages
{
    public const string EmptyFile = "Profile image file is required.";
    public const string TooLarge = "Profile image must be at most 2 MB.";
    public const string InvalidContentType = "Profile image must be JPEG, PNG, or WebP.";
    public const string InvalidContent = "Profile image content is invalid.";
}
