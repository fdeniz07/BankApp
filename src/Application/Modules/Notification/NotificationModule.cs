using Microsoft.Extensions.DependencyInjection;

namespace Application.Modules.Notification;

/// <summary>
/// Notification bounded context (application side): messaging contracts used by the host
/// (email confirmation publisher, realtime notifications implemented in WebApi).
/// </summary>
public static class NotificationModule
{
    public const string Name = "Notification";

    public static IServiceCollection AddNotificationModule(this IServiceCollection services)
    {
        // Host (WebApi) registers RabbitMQ publishers/consumers and SignalR notifiers.
        return services;
    }
}
