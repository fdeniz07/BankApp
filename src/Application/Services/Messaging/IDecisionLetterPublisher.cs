namespace Application.Services.Messaging;

public interface IDecisionLetterPublisher
{
    Task PublishAsync(DecisionLetterRequested message, CancellationToken cancellationToken = default);
}
