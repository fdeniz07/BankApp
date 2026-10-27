namespace Application.Services.Messaging;

public interface IDecisionSmsPublisher
{
    Task PublishAsync(DecisionSmsRequested message, CancellationToken cancellationToken = default);
}
