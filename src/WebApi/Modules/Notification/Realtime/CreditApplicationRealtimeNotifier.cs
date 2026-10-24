using Microsoft.AspNetCore.SignalR;

namespace WebApi.Modules.Notification.Realtime;

public sealed class CreditApplicationRealtimeNotifier : ICreditApplicationRealtimeNotifier
{
    private readonly IHubContext<CreditApplicationsHub> _hub;
    private readonly ILogger<CreditApplicationRealtimeNotifier> _logger;

    public CreditApplicationRealtimeNotifier(
        IHubContext<CreditApplicationsHub> hub,
        ILogger<CreditApplicationRealtimeNotifier> logger)
    {
        _hub = hub;
        _logger = logger;
    }

    public async Task NotifyAsync(
        CreditApplicationChangedMessage message,
        CancellationToken cancellationToken = default)
    {
        await _hub.Clients
            .Group(CreditApplicationRealtimeGroups.Staff)
            .SendAsync(CreditApplicationsHub.CreditApplicationChanged, message, cancellationToken);

        await _hub.Clients
            .Group(CreditApplicationRealtimeGroups.Customer(message.CustomerId))
            .SendAsync(CreditApplicationsHub.CreditApplicationChanged, message, cancellationToken);

        _logger.LogInformation(
            "Realtime notified. EventType={EventType} ApplicationId={ApplicationId} CustomerId={CustomerId}",
            message.EventType,
            message.ApplicationId,
            message.CustomerId);
    }
}
