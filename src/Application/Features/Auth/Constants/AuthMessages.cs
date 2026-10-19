namespace Application.Features.Auth.Constants;

public static class AuthMessages
{
    public const string UserNotFound = "User not found.";
    public const string PasswordIncorrect = "Password is incorrect.";
    public const string EmailAlreadyExists = "Email already exists.";
    public const string UserNotActive = "User is not active.";
    public const string CustomerNotFound = "Customer not found for the given CustomerId.";
    public const string EmailNotConfirmed = "Email address is not confirmed. Please check your inbox.";
    public const string InvalidOrExpiredConfirmationToken = "Confirmation link is invalid or expired.";
    public const string EmailAlreadyConfirmed = "Email address is already confirmed.";
}
