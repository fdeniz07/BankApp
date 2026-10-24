namespace Application.Modules.Credit.Sagas;

public interface ICreditApplicationProcessManager
{
    Task HandleAsync(string eventType, string payload, CancellationToken cancellationToken = default);
}
