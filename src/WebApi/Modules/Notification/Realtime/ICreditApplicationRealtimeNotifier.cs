namespace WebApi.Modules.Notification.Realtime;

public interface ICreditApplicationRealtimeNotifier
{
    Task NotifyAsync(CreditApplicationChangedMessage message, CancellationToken cancellationToken = default);
}
