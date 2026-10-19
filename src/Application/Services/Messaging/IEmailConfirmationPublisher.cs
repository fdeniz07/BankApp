namespace Application.Services.Messaging;

public interface IEmailConfirmationPublisher
{
    Task PublishAsync(EmailConfirmationRequested message, CancellationToken cancellationToken = default);
}
