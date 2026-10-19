namespace Application.Services.Messaging;

public sealed record EmailConfirmationRequested(
    Guid UserId,
    string Email,
    string Token);
